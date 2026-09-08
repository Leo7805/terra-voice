namespace TerraVoice.Api.Dtos;

/// <summary>
///     Represents aggregated usage statistics for a given provider and month.
/// </summary>
/// <param name="Provider">
///     The TTS provider name (e.g., "Azure", "OpenAI").
/// </param>
/// <param name="Month">
///     The aggregation period in "yyyy-MM" format (e.g., "2026-05").
/// </param>
/// <param name="RequestCount">
///     Total number of TTS requests made during the month.
/// </param>
/// <param name="CharCount">
///     Total number of characters processed by TTS during the month.
/// </param>
/// <param name="QuotaCharLimit">
///     Monthly character quota limit configured for the provider.
/// </param>
/// <param name="RemainingCharCount">
///     Remaining characters available for the current month (Quota - Used).
/// </param>
/// <example>
///     Example JSON response:
///     <code>
/// {
///   "provider": "Azure",
///   "month": "2026-05",
///   "requestCount": 120,
///   "charCount": 45000,
///   "quotaCharLimit": 100000,
///   "remainingCharCount": 55000
/// }
/// </code>
/// </example>
public sealed record UsageSummaryResponse(
    string Provider,
    string Month,
    int RequestCount,
    int CharCount,
    int QuotaCharLimit,
    int RemainingCharCount
);