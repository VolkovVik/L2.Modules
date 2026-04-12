using Aspu.Api.Options;

namespace Aspu.Api.Extensions;

internal static class OptionsExtensions
{
    internal static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MqttOptions>(configuration.GetSection(MqttOptions.SectionName));
        services.Configure<ApiVersionOptions>(configuration.GetSection(ApiVersionOptions.SectionName));

        return services;
    }
}
