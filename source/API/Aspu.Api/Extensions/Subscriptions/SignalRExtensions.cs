using System.Text.Json;
using Aspu.Api.Adapters.Signalr;
using Aspu.Api.Options;
using Aspu.Common.Application.Ports.SignalrPort;
using Aspu.Common.SourceGenerators.Application;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Aspu.Api.Extensions.Subscriptions;

internal static class SignalrExtensions
{
    internal static IServiceCollection AddSignalRSubscriber(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(SignalrOptions.SectionName).Get<SignalrOptions>();
        if (options?.Enabled is not true)
            return services;

        services
            .AddSignalR()
            .AddJsonProtocol(options =>
            {
                // SignalR serializes via Type + resolver chain; JsonSourceGenerationOptions apply only
                // when PayloadSerializerOptions is aligned with the source-generated context options.
                options.PayloadSerializerOptions = new JsonSerializerOptions(SignalrJsonContext.Default.Options);
            });

        services.AddSingleton<SignalrNotificationChannel>();
        services.AddSingleton<ISignalrNotificationChannel>(sp => sp.GetRequiredService<SignalrNotificationChannel>());
        services.AddHostedService<SignalrMessageWorker>();
        services.AddSingleton<ISignalrNotificationPublisher>(sp =>
        {
            var hubContext = sp.GetRequiredService<IHubContext<SignalrNotificationsHub, ISignalrNotificationsHub>>();
            return SignalrNotificationPublisherRegistration.Create(
                static (host) => SignalrNotificationsHub.GetConnectionId(host),
                () => hubContext.Clients.All,
                (connectionId) => hubContext.Clients.Client(connectionId!),
                (audience) => hubContext.Clients.Group(audience!));
        });

        return services;
    }

    internal static WebApplication MapSignalRHub(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<SignalrOptions>>().Value;
        if (options?.Enabled is not true)
            return app;

        app.MapHub<SignalrNotificationsHub>(options.HubPath);
        return app;
    }
}
