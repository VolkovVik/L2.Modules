using Aspu.Common.Application.Ports.SignalrPort;

namespace Aspu.Api.Adapters.SignalR;

public interface INotificationsClient
{
    Task Test1Notification(Test1Notification notification);

    Task Test2Notification(Test2Notification notification);
}
