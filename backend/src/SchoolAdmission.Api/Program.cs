using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Errors;
using SchoolAdmission.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Connection string 'Default' is not set. Set the ConnectionStrings__Default environment variable.");

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(connectionString)
    .UseSnakeCaseNamingConvention());

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ClassService>();

builder.Services.AddApiProblemDetails();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
        options.AllowInputFormatterExceptionMessages = false;
    });

var app = builder.Build();

await DatabaseMigrator.MigrateAsync(app.Services);

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapControllers();

app.Run();
