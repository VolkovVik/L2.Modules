using Aspu.Api.Adapters.SignalR;
using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.SignalR;
using Microsoft.Extensions.Options;

namespace Aspu.Api.Extensions.Subscriptions;

internal static class SignalRExtensions
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
            .AddJsonProtocol(jsonProtocolOptions =>
            {
                jsonProtocolOptions.PayloadSerializerOptions.TypeInfoResolverChain.Insert(
                    0,
                    SignalRJsonContext.Default);
            });

        services.AddSingleton<SignalrNotificationChannel>();
        services.AddSingleton<ISignalrNotificationChannel>(sp => sp.GetRequiredService<SignalrNotificationChannel>());
        services.AddHostedService<SignalrMessageWorker>();
        services.AddSingleton<INotificationPublisher, SignalRNotificationPublisher>();

        return services;
    }

    internal static WebApplication MapSignalRHub(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<SignalROptions>>().Value;
        if (options?.Enabled is not true)
            return app;

        app.MapHub<NotificationsHub>(options.HubPath);
        return app;
    }
}
