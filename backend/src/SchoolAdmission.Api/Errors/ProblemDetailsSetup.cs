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
            var problem = new ValidationProblemDetails(context.ModelState)
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
}
