namespace Aspu.Common.Presentation.Abstractions.SignalR;

public sealed record Test1Notification(string Description, DateTime Timestamp) : ISignalrNotification;
