using System.Text.Json.Serialization;
using Aspu.Api.Options;
using Aspu.Api.Ports.Signalr;
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
        var options = configuration.GetSection(SignalROptions.SectionName).Get<SignalROptions>();
        if (options?.Enabled is not true)
            return services;

        services
            .AddSignalR()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, SignalrJsonContext.Default);
                options.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
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
                (connectionId) => hubContext.Clients.Client(connectionId!));
        });

        return services;
    }

    internal static WebApplication MapSignalRHub(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<SignalROptions>>().Value;
        if (options?.Enabled is not true)
            return app;

        app.MapHub<SignalrNotificationsHub>(options.HubPath);
        return app;
    }
}
