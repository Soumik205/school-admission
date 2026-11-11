using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SchoolAdmission.Api.Data;

public static class DatabaseMigrator
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    // Compose waits for the db healthcheck, but Postgres can still refuse
    // connections for a moment after it reports ready, so retry briefly.
    public static async Task MigrateAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                logger.LogInformation("Database is up to date");
                return;
            }
            catch (NpgsqlException ex) when (attempt < MaxAttempts)
            {
                logger.LogWarning("Database not reachable (attempt {Attempt} of {MaxAttempts}): {Message}",
                    attempt, MaxAttempts, ex.Message);
                await Task.Delay(RetryDelay);
            }
        }
    }
}
