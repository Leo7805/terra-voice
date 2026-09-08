using TerraVoice.Api.Errors;

namespace TerraVoice.Api.Configuration;

/// <summary>
///     Azure Speech configuration.
///     Values are loaded from appsettings, user-secrets, and environment variables.
/// </summary>
public sealed class AzureSpeechSettings
{
    public string Key { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Key))
            throw new ConfigurationException("Missing AzureSpeech Setting: Key configuration.");

        if (string.IsNullOrWhiteSpace(Region))
            throw new ConfigurationException("Missing AzureSpeech Setting: Region configuration.");
    }
}