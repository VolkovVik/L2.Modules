using Aspu.Common.Domain.Results;
using Mediator;

namespace Aspu.Common.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
