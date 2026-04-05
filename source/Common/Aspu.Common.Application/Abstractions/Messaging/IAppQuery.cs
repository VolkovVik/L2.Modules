using Aspu.Common.Domain.Results;
using Mediator;

namespace Aspu.Common.Application.Abstractions.Messaging;

public interface IAppQuery<TResponse> : IRequest<Result<TResponse>>;
