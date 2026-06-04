namespace Aspu.Common.Application.Ports.SignalrPort;

public interface ISignalrNotificationsHub
{
    Task Test1Notification(Test1Notification notification);

    Task Test2Notification(Test2Notification notification);
}
