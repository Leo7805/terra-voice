namespace TerraVoice.Api.Configuration;

public sealed class UsageSettings
{
    public string Provider { get; set; } = "azure";
    public int MonthlyCharQuota { get; set; } = 500000;
}