using TerraVoice.Api.Errors;

namespace TerraVoice.Api.Configuration;

/// <summary>
///     TTS application settings (business rules & defaults).
/// </summary>
public sealed class TtsSettings
{
    private const decimal MinSpeakingRate = 0.5m;
    private const decimal MaxSpeakingRate = 2.0m;

    private const int MinTextLength = 100;
    public int MaxTextLength { get; set; } = 3000;

    public string DefaultEnglishVoiceName { get; set; } = "";
    public string DefaultChineseVoiceName { get; set; } = "";

    public decimal DefaultSpeakingRate { get; set; } = 1.0m;

    public void Validate()
    {
        if (MaxTextLength < MinTextLength)
            throw new ConfigurationException($"Tts:MaxTextLength must be at least {MinTextLength}.");
        if (string.IsNullOrWhiteSpace(DefaultEnglishVoiceName))
            throw new ConfigurationException("Missing Tts:DefaultEnglishVoiceName configuration.");

        if (string.IsNullOrWhiteSpace(DefaultChineseVoiceName))
            throw new ConfigurationException("Missing Tts:DefaultChineseVoiceName configuration.");

        if (DefaultSpeakingRate is < MinSpeakingRate or > MaxSpeakingRate)
            throw new ConfigurationException(
                $"Tts:DefaultSpeakingRate must be between {MinSpeakingRate} and {MaxSpeakingRate}.");
    }
}