using Aspu.Common.Domain.Results;
using Mediator;

namespace Aspu.Common.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
