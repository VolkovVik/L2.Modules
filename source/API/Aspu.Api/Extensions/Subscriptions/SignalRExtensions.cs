using Aspu.Api.Adapters.SignalR;
using Aspu.Api.Options;
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

        services.AddSignalR();
        services.AddSingleton<SignalrMessageChannel>();
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
