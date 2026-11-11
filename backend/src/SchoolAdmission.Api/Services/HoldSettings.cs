namespace SchoolAdmission.Api.Services;

public record HoldSettings(TimeSpan HoldDuration)
{
    public const int DefaultHoldDurationSeconds = 600;

    public static HoldSettings FromConfiguration(IConfiguration configuration)
    {
        var raw = configuration["HOLD_DURATION_SECONDS"];
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new HoldSettings(TimeSpan.FromSeconds(DefaultHoldDurationSeconds));
        }
        if (!int.TryParse(raw, out var seconds) || seconds <= 0)
        {
            throw new InvalidOperationException(
                $"HOLD_DURATION_SECONDS must be a positive whole number of seconds, but was '{raw}'.");
        }
        return new HoldSettings(TimeSpan.FromSeconds(seconds));
    }
}
