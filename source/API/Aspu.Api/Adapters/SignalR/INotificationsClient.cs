using Aspu.Common.Presentation.Abstractions.SignalR;

namespace Aspu.Api.Adapters.SignalR;

public interface INotificationsClient
{
    Task Test1Notification(Test1Notification notification);

    Task Test2Notification(Test2Notification notification);
}
