using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace Aspu.Api.Middleware;

public sealed class ValidationBehavior<TMessage, TResponse>(
    IEnumerable<IValidator<TMessage>> _validators) :
    IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(message, cancellationToken);

        var context = new ValidationContext<TMessage>(message);
        var failures = new List<ValidationFailure>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            if (!result.IsValid)
                failures.AddRange(result.Errors);
        }

        return failures.Count > 0
            ? throw new ValidationException(failures)
            : await next(message, cancellationToken);
    }
}
