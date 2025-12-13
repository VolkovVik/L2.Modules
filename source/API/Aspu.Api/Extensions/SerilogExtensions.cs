using System.Globalization;
using Serilog;

namespace Aspu.Api.Extensions;

internal static class SerilogExtensions
{
    /// <remarks>
    /// https://nblumhardt.com/2020/10/bootstrap-logger/
    /// https://nblumhardt.com/2024/04/serilog-net8-0-minimal/
    /// </remarks>

    internal static void AddDefaultLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();
    }

    internal static IServiceCollection AddLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSerilog((provider, loggerConfiguration) =>
            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(provider)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId());
        return services;
    }
}
