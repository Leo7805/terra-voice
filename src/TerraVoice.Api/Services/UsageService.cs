using Microsoft.Extensions.Options;
using TerraVoice.Api.Configuration;
using TerraVoice.Api.Data;
using TerraVoice.Api.Dtos;

namespace TerraVoice.Api.Services;

public sealed class UsageService
{
    private const string MonthFormat = "yyyy-MM";
    private readonly UsageSettings _settings;

    private readonly UsageDb _usageDb;

    public UsageService(UsageDb usageDb, IOptions<UsageSettings> usageOptions)
    {
        _usageDb = usageDb;
        _settings = usageOptions.Value;
    }

    public Task RecordAsync(int charCount)
    {
        var month = GetCurrentMonth();

        return _usageDb.RecordAsync(
            _settings.Provider,
            month,
            charCount
        );
    }

    public async Task<UsageSummaryResponse> GetCurrentMonthSummaryAsync()
    {
        var month = GetCurrentMonth();
        var usage = await _usageDb.GetMonthlyAsync(_settings.Provider, month);

        var quota = _settings.MonthlyCharQuota;
        var remaining = Math.Max(0, quota - usage.CharCount);

        return new UsageSummaryResponse(
            _settings.Provider,
            month,
            usage.RequestCount,
            usage.CharCount,
            quota,
            remaining
        );
    }

    private static string GetCurrentMonth()
    {
        return DateTime.UtcNow.ToString(MonthFormat);
    }
}