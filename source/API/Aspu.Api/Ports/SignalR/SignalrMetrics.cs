using System.Diagnostics.Metrics;

namespace Aspu.Api.Ports.Signalr;

public sealed class SignalrMetrics
{
    public const string MeterName = "Aspu.SignalR";

    private static readonly Meter s_meter = new(MeterName);

    private readonly Counter<long> _enqueued;
    private readonly Counter<long> _dropped;
    private readonly UpDownCounter<long> _connectedClients;

    public SignalrMetrics()
    {
        _enqueued = s_meter.CreateCounter<long>(
            "signalr_notifications_enqueued",
            unit: "{notification}",
            description: "Notifications accepted into the SignalR channel.");
        _dropped = s_meter.CreateCounter<long>(
            "signalr_notifications_dropped",
            unit: "{notification}",
            description: "Notifications rejected because the SignalR channel is full.");
        _connectedClients = s_meter.CreateUpDownCounter<long>(
            "signalr_connected_clients",
            unit: "{client}",
            description: "Current number of connected SignalR clients.");
    }

    public void RecordEnqueued() => _enqueued.Add(1);

    public void RecordDropped() => _dropped.Add(1);

    public void ClientConnected() => _connectedClients.Add(1);

    public void ClientDisconnected() => _connectedClients.Add(-1);
}
