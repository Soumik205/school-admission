using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Connection string 'Default' is not set. Set the ConnectionStrings__Default environment variable.");

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(connectionString)
    .UseSnakeCaseNamingConvention());

builder.Services.AddControllers();

var app = builder.Build();

await DatabaseMigrator.MigrateAsync(app.Services);

app.MapControllers();

app.Run();
