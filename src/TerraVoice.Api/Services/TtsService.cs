using System.Security;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Options;
using TerraVoice.Api.Configuration;
using TerraVoice.Api.Dtos;
using TerraVoice.Api.Errors;

namespace TerraVoice.Api.Services;

public class TtsService
{
    private const decimal MinSpeakingRate = 0.5m;
    private const decimal MaxSpeakingRate = 2.0m;
    private const decimal DefaultSpeakingRate = 1.0m;
    private readonly AzureSpeechSettings _azureSpeechSettings;
    private readonly TtsSettings _ttsSettings;

    public TtsService(
        IOptions<AzureSpeechSettings> azureSpeechOptions,
        IOptions<TtsSettings> ttsOptions)
    {
        _ttsSettings = ttsOptions.Value;
        _azureSpeechSettings = azureSpeechOptions.Value;

        _ttsSettings.Validate();
        _azureSpeechSettings.Validate();
    }

    public async Task<byte[]> SynthesizeAsync(TtsRequest request)
    {
        // 1. validate
        ValidateRequest(request);
        var text = request.Text.Trim();

        // 2. Speaking Rate
        var speakingRate = request.SpeakingRate ?? _ttsSettings.DefaultSpeakingRate;

        // 3. Choose voice
        var voiceName = ChooseVoice(request, text);

        // 4a. Call Azure TTS using text and return audio bytes
        if (speakingRate == DefaultSpeakingRate)
            return await SynthesizeAudioAsync(text, voiceName);

        // 4b. Call Azure using ssml and return audio bytes
        var ssml = BuildSsml(text, voiceName, speakingRate);
        return await SynthesizeAudioAsync(ssml, voiceName, true);
    }

    private void ValidateRequest(TtsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text)) throw new BadRequestException("Text is required.");

        var text = request.Text.Trim();

        if (text.Length > _ttsSettings.MaxTextLength)
            throw new BadRequestException(
                $"Text is too long. Maximum length is {_ttsSettings.MaxTextLength} characters.");

        if (request.SpeakingRate is < MinSpeakingRate or > MaxSpeakingRate)
            throw new BadRequestException($"SpeakingRate must be between {MinSpeakingRate} and {MaxSpeakingRate}.");
    }

    private static string BuildSsml(string text, string voiceName, decimal speakingRate = DefaultSpeakingRate)
    {
        var escapedText = SecurityElement.Escape(text);
        // var ratePercent = $"{speakingRate * 100:0}%";
        var lang = GetLanguageFromVoice(voiceName);

        return $"""
                <speak version="1.0" xml:lang="{lang}">
                  <voice name="{voiceName}">
                    <prosody rate="{speakingRate}">
                      {escapedText}
                    </prosody>
                  </voice>
                </speak>
                """;
    }

    private static bool ContainsChinese(string text)
    {
        return text.Any(c =>
            (c >= '\u4e00' && c <= '\u9fff') ||
            (c >= '\u3400' && c <= '\u4dbf')
        );
    }

    private string ChooseVoice(TtsRequest request, string text)
    {
        var isChinese = ContainsChinese(text);

        if (isChinese)
        {
            if (IsChineseVoice(request.VoiceName))
                return request.VoiceName!;

            if (IsChineseVoice(request.SecondaryVoiceName))
                return request.SecondaryVoiceName!;

            return _ttsSettings.DefaultChineseVoiceName;
        }

        if (IsEnglishVoice(request.VoiceName))
            return request.VoiceName!;

        if (IsEnglishVoice(request.SecondaryVoiceName))
            return request.SecondaryVoiceName!;

        return _ttsSettings.DefaultEnglishVoiceName;
    }

    private static bool IsChineseVoice(string? voiceName)
    {
        return !string.IsNullOrWhiteSpace(voiceName)
               && voiceName.StartsWith("zh-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEnglishVoice(string? voiceName)
    {
        return !string.IsNullOrWhiteSpace(voiceName)
               && voiceName.StartsWith("en-", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetLanguageFromVoice(string voiceName)
    {
        if (voiceName.StartsWith("zh-", StringComparison.OrdinalIgnoreCase))
            return "zh-CN";

        if (voiceName.StartsWith("en-", StringComparison.OrdinalIgnoreCase))
            return "en-US";

        // fallback
        return "en-US";
    }

    private async Task<byte[]> SynthesizeAudioAsync(string input, string voiceName, bool useSsml = false)
    {
        var speechConfig = SpeechConfig.FromSubscription(_azureSpeechSettings.Key, _azureSpeechSettings.Region);

        if (!useSsml) speechConfig.SpeechSynthesisVoiceName = voiceName;

        // MP3 is smaller and tends to be more robust across browser playback pipelines.
        speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio24Khz48KBitRateMonoMp3);

        // Disable local speaker output on the backend process.
        // We only need the synthesized bytes returned to the frontend.
        // audioConfig: null, do not play using local device
        using var synthesizer = new SpeechSynthesizer(speechConfig, null);

        // Synthesize the speech
        var result = useSsml
            ? await synthesizer.SpeakSsmlAsync(input)
            : await synthesizer.SpeakTextAsync(input);

        // Check if synthesis succeeded
        if (result.Reason != ResultReason.SynthesizingAudioCompleted)
        {
            // Extract detailed error info from Azure
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);

            var detail = string.IsNullOrWhiteSpace(cancellation.ErrorDetails)
                ? "Speech synthesis failed."
                : cancellation.ErrorDetails;

            throw new ExternalServiceException(detail);
        }

        // Validate audio data (defensive check)
        if (result.AudioData is null || result.AudioData.Length == 0)
            throw new ExternalServiceException("Speech synthesis returned empty data.");

        // Return raw audio bytes (MPEG)
        return result.AudioData;
    }
}