using Mediator;

namespace Aspu.Common.Application.Abstractions.Messaging;

public interface IAppNotificationHandler<in TNotification> : INotificationHandler<TNotification>
    where TNotification : IAppNotification;
