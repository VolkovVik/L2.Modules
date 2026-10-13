using Aspu.Common.Domain.Results;
using Mediator;

namespace Aspu.Common.Application.Abstractions.Messaging;

public interface IAppBaseCommand;

public interface IAppCommand : IRequest<AppResult>, IAppBaseCommand;

public interface IAppCommand<TResponse> : IRequest<AppResult<TResponse>>, IAppBaseCommand;
