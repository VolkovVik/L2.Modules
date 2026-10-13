using Aspu.Common.Domain.Results;
using Mediator;

namespace Aspu.Common.Application.Abstractions.Messaging;

public interface IAppQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, AppResult<TResponse>>
    where TQuery : IAppQuery<TResponse>;
