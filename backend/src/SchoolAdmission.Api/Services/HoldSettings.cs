namespace SchoolAdmission.Api.Services;

public record HoldSettings(TimeSpan HoldDuration, TimeSpan SweepInterval)
{
    public static HoldSettings FromConfiguration(IConfiguration configuration) => new(
        ReadSeconds(configuration, "HOLD_DURATION_SECONDS", 600),
        ReadSeconds(configuration, "EXPIRY_SWEEP_INTERVAL_SECONDS", 15));

    private static TimeSpan ReadSeconds(IConfiguration configuration, string key, int defaultSeconds)
    {
        var raw = configuration[key];
        if (string.IsNullOrWhiteSpace(raw))
        {
            return TimeSpan.FromSeconds(defaultSeconds);
        }
        if (!int.TryParse(raw, out var seconds) || seconds <= 0)
        {
            throw new InvalidOperationException($"{key} must be a positive whole number of seconds, but was '{raw}'.");
        }
        return TimeSpan.FromSeconds(seconds);
    }
}
