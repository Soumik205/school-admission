using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SchoolAdmission.Api.Errors;

public static class ProblemDetailsSetup
{
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            // Framework-generated problems (unknown route, unsupported media type) carry no code of their own.
            var extensions = context.ProblemDetails.Extensions;
            if (!extensions.ContainsKey("code"))
            {
                extensions["code"] = context.ProblemDetails.Status switch
                {
                    StatusCodes.Status404NotFound => ErrorCodes.NotFound,
                    >= 500 => ErrorCodes.InternalError,
                    _ => ErrorCodes.BadRequest,
                };
            }
            extensions.Remove("traceId");
        });

        services.PostConfigure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = context =>
        {
            var entries = context.ModelState.Where(entry => entry.Value is { Errors.Count: > 0 }).ToList();

            // When the JSON itself is broken, the binder also reports the whole body as missing,
            // which only repeats the real problem.
            if (entries.Any(entry => entry.Key.StartsWith('$')))
            {
                entries = entries.Where(entry => entry.Key.StartsWith('$')).ToList();
            }

            var errors = entries.ToDictionary(
                entry => FieldName(entry.Key),
                entry => entry.Value!.Errors
                    .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "The value is not valid." : e.ErrorMessage)
                    .ToArray());
            var problem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = "One or more fields are invalid.",
                Extensions = { ["code"] = ErrorCodes.ValidationFailed },
            };
            return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
        });

        return services;
    }

    private static string FieldName(string modelStateKey) => modelStateKey switch
    {
        "" or "$" => "body",
        _ when modelStateKey.StartsWith("$.") => modelStateKey[2..],
        _ => JsonNamingPolicy.CamelCase.ConvertName(modelStateKey),
    };
}
