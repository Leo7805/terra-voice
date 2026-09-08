namespace TerraVoice.Api.Dtos;

/// <summary>
///     Request model for Text-to-Speech (TTS) synthesis.
/// </summary>
/// <param name="Text">
///     The input text to be synthesized into speech.
/// </param>
/// <param name="VoiceName">
///     Optional primary Azure voice name (e.g., "en-AU-NatashaNeural").
///     If not provided, the system will fall back to the default voice
///     configured in appsettings or user secrets.
/// </param>
/// <param name="SecondaryVoiceName">
///     Optional secondary Azure voice name used for mixed‑language or
///     multi‑speaker synthesis scenarios. If omitted, only the primary
///     voice will be used.
/// </param>
/// <param name="SpeakingRate">
///     Optional speaking‑rate multiplier applied to the synthesized speech.
///     A value of 1.0 represents normal speed. Values below 1.0 slow down
///     the speech, and values above 1.0 speed it up. Recommended range:
///     0.5 to 2.0.
/// </param>
public sealed record TtsRequest(
    string Text,
    string? VoiceName = null,
    string? SecondaryVoiceName = null,
    decimal? SpeakingRate = null
);