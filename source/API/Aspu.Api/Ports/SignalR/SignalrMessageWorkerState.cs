namespace Aspu.Api.Ports.Signalr;

internal sealed class SignalrMessageWorkerState
{
    public bool IsFaulted => Fault is not null;

    public Exception? Fault { get; private set; }

    public void ClearFault() => Fault = null;

    public void SetFault(Exception exception) => Fault = exception;
}
