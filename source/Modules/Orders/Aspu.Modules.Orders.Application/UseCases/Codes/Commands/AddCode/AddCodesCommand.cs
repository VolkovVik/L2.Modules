using Aspu.Common.Application.Abstractions.Messaging;
using Aspu.Common.Application.Extensions;
using Aspu.Common.Domain.Results;
using Aspu.Modules.Orders.Domain.Model.CodeAggregate;
using FluentValidation;
using Mediator;
using Serilog;

namespace Aspu.Modules.Orders.Application.UseCases.Codes.Commands.AddCode;

public sealed record AddCodeCommand(Guid? OrderId, Guid? OrderUnitId, string Value)
    : IAppCommand<Guid>;

internal sealed class AddCodeCommandValidator : AbstractValidator<AddCodeCommand>
{
    public AddCodeCommandValidator()
    {
        RuleFor(c => c.OrderId)
            .NotEmpty();
        RuleFor(c => c.OrderUnitId)
            .NotEmpty();
        RuleFor(c => c.Value)
            .StringMustBeNotWhiteSpace();
    }
}

public sealed class AddCodeCommandHandler(IMediator _mediator)
    : IAppCommandHandler<AddCodeCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(AddCodeCommand request, CancellationToken cancellationToken)
    {
        var code = Code.Create(request.OrderId!.Value, request.OrderUnitId!.Value, request.Value);
        if (code.IsFailure)
            return code.Error;

        await _mediator.Publish(new CodeNotification(code.Value.Id), cancellationToken);

        return code.Value.Id;
    }
}

public sealed record CodeNotification(Guid Id) : IAppNotification;

public sealed class CodeNotificationHandler : IAppNotificationHandler<CodeNotification>
{
    public ValueTask Handle(CodeNotification notification, CancellationToken cancellationToken)
    {
        Log.Debug("Add code: {@Id}", notification.Id);
        return ValueTask.CompletedTask;
    }
}
