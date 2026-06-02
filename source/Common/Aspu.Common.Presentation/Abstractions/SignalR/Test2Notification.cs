namespace Aspu.Common.Presentation.Abstractions.SignalR;

public sealed record Test2Notification(string ErrorId, int Value, DateTime Timestamp) : ISignalrNotification;
