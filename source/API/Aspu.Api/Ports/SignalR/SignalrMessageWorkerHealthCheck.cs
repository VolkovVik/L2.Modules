using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Aspu.Api.Ports.Signalr;

internal sealed class SignalrMessageWorkerHealthCheck(SignalrMessageWorkerState workerState) : IHealthCheck
{
    public const string Name = "signalr_worker";

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (workerState.IsFaulted)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "SignalR message worker is in a faulted state.",
                workerState.Fault));
        }

        return Task.FromResult(HealthCheckResult.Healthy("SignalR message worker is running."));
    }
}
