using Aspu.Common.Domain.Results;
using Mediator;

namespace Aspu.Common.Application.Abstractions.Messaging;

public interface IBaseCommand;

public interface ICommand : IRequest<Result>, IBaseCommand;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;
