using Aspu.Common.Domain.Results;
using Mediator;

namespace Aspu.Common.Application.Abstractions.Messaging;

public interface IAppCommandHandler<in TCommand> : IRequestHandler<TCommand, AppResult>
    where TCommand : IAppCommand;

public interface IAppCommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, AppResult<TResponse>>
    where TCommand : IAppCommand<TResponse>;
