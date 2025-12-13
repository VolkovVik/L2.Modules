using Microsoft.AspNetCore.Http.Features;

namespace Aspu.Api.Extensions.Exceptions;

internal static class ExceptionExtensions
{
    ///<remarks>
    /// https://www.milanjovanovic.tech/blog/problem-details-for-aspnetcore-apis
    ///</remarks>

    internal static IServiceCollection AddExceptionHandlers(
        this IServiceCollection services)
    {
        services.AddExceptionHandler<UnauthorizedAccessExceptionHandler>();
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
            };
        });
        return services;
    }
}
