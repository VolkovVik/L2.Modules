using Aspu.Common.Application.Abstractions.Messaging;
using Aspu.Common.Application.Extensions;
using Aspu.Common.Domain.Results;
using Aspu.Modules.Orders.Domain.Model.CodeAggregate;
using FluentValidation;
using Mediator;

namespace Aspu.Modules.Orders.Application.UseCases.Codes.Commands.ReplaceCode;

public sealed record ReplaceCodeCommand(Guid CodeId, string NewValue)
    : IAppCommand<Guid>;

internal sealed class ReplaceCodeCommandValidator : AbstractValidator<ReplaceCodeCommand>
{
    public ReplaceCodeCommandValidator()
    {
        RuleFor(c => c.CodeId)
            .NotEmpty();
        RuleFor(c => c.NewValue)
            .StringMustBeNotWhiteSpace();
    }
}

#pragma warning disable CS9113 // Parameter is unread.
public sealed class ReplaceCodeCommandHandler(IMediator _mediator)
#pragma warning restore CS9113 // Parameter is unread.
    : IAppCommandHandler<ReplaceCodeCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(ReplaceCodeCommand request, CancellationToken cancellationToken)
    {
        var code = Code.Create(Guid.NewGuid(), Guid.NewGuid(), request.NewValue);
        if (code.IsFailure)
            return code.Error;

        return request.CodeId;
    }
}
