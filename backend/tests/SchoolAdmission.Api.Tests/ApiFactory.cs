using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using SchoolAdmission.Api.Data;

namespace SchoolAdmission.Api.Tests;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
        ?? "Host=localhost;Port=5433;Database=school_admission_test;Username=postgres;Password=postgres;Maximum Pool Size=50";

    public FakeTimeProvider Time { get; } = new(DateTimeOffset.UtcNow);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", ConnectionString);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Time);
        });
    }

    // Start every run from an empty database; the app applies migrations when the host starts.
    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(ConnectionString).Options;
        await using var db = new AppDbContext(options);
        await db.Database.EnsureDeletedAsync();
        _ = Server;
    }

    public new Task DisposeAsync() => base.DisposeAsync().AsTask();
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
