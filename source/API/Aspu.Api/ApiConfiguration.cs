using Aspu.Api.Adapters.Mqtt;
using Aspu.Api.Options;
using Aspu.Api.SourceGenerators.Endpoints;
using Aspu.Api.SourceGenerators.Validators;

namespace Aspu.Api;

public static class ApiConfiguration
{
    public static IServiceCollection AddApiModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidators();

        var mqttOptions = configuration.GetSection(MqttOptions.SectionName).Get<MqttOptions>();
        if (mqttOptions?.Enabled == true)
            services.AddMqttSubscriber();

        return services;
    }

    private static IServiceCollection AddMqttSubscriber(
        this IServiceCollection services)
    {
        services.AddSingleton<MqttMessageHandlerTopicRegistry>();
        services.AddSingleton<MqttInboundMessageQueue>();
        services.AddSingleton<MqttSubscriberClient>();
        // Processor stops after subscriber (reverse registration): subscriber completes the channel writer on exit.
        services.AddHostedService<MqttInboundMessageHostedService>();
        services.AddHostedService<MqttSubscriberHostedService>();

        return services;
    }

    public static IEndpointRouteBuilder MapApiEndpoints(
        this IEndpointRouteBuilder app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        MapHelloEndpoint(app);
        EndpointsRegistration.MapEndpoints(app, routeGroupBuilder);

        return routeGroupBuilder ?? app;
    }

    private static void MapHelloEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/", () => "Hello from ASPU.API")
            .AllowAnonymous()
            .WithName("Hello")
            .WithSummary("Hello")
            .WithDescription("Return hello message")
            .WithTags("Hello");
}
