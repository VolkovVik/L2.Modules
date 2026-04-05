using Aspu.Common.Application.Abstractions.Messaging;
using Aspu.Common.Domain.Results;
using Aspu.Modules.Orders.Domain.Model.CodeAggregate;
using FluentValidation;
using Mediator;
using Serilog;

namespace Aspu.Modules.Orders.Application.UseCases.Codes.Commands.GetCodeById;

public sealed record GetCodeByIdCommand(Guid? Value)
    : IAppCommand<Guid>;

internal sealed class GetCodeByIdCommandValidator : AbstractValidator<GetCodeByIdCommand>
{
    public GetCodeByIdCommandValidator() =>
        RuleFor(c => c.Value)
            .NotNull()
            .NotEmpty();
}

public sealed class GetCodeByIdCommandHandler(IMediator _mediator)
    : IAppCommandHandler<GetCodeByIdCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(GetCodeByIdCommand request, CancellationToken cancellationToken)
    {
        var code = Code.Create(Guid.NewGuid(), Guid.NewGuid(), "test");
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
        Log.Debug("Get code by ID: {@Id}", notification.Id);
        return ValueTask.CompletedTask;
    }
}
