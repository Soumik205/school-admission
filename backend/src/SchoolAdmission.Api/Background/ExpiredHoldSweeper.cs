using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Domain;
using SchoolAdmission.Api.Services;

namespace SchoolAdmission.Api.Background;

// Only tidies stored rows. Seat counts and statuses are already correct without it, because reads
// treat a HELD hold past its expiry as EXPIRED. Safe to run from several processes at once.
public class ExpiredHoldSweeper(
    IServiceScopeFactory scopeFactory,
    TimeProvider time,
    HoldSettings settings,
    ILogger<ExpiredHoldSweeper> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(settings.SweepInterval, time);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SweepAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Expired hold sweep failed; will retry on the next tick");
            }
        }
    }

    public async Task<int> SweepAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = time.GetUtcNow().UtcDateTime;

        var expired = await db.Holds
            .Where(h => h.Status == HoldStatus.Held && h.ExpiresAt <= now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(h => h.Status, HoldStatus.Expired)
                .SetProperty(h => h.UpdatedAt, now), cancellationToken);

        if (expired > 0)
        {
            logger.LogInformation("Marked {Count} holds as expired", expired);
        }
        return expired;
    }
}
