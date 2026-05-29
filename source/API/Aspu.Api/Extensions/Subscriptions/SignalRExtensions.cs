using Aspu.Api.Adapters.SignaR;

namespace Aspu.Api.Extensions.Subscriptions;

internal static class SignalRExtensions
{
    internal static IServiceCollection AddSignalRSubscriber(
        this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<SignalrMessageChannel>();
        services.AddHostedService<SignalrMessageWorker>();
        services.AddScoped<INotificationPublisher, SignalRNotificationPublisher>();

        return services;
    }
}
