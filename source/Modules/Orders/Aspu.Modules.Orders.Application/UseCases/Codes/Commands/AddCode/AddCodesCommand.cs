using Aspu.Common.Application.Abstractions.Messaging;
using Aspu.Common.Application.Extensions;
using Aspu.Common.Application.Ports.MessageBus;
using Aspu.Common.Domain.Results;
using Aspu.Modules.Orders.Domain.Model.CodeAggregate;
using Aspu.Modules.Orders.IntegrationEvents.Codes;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Aspu.Modules.Orders.Application.UseCases.Codes.Commands.AddCode;

public sealed record AddCodeCommand(Guid? OrderId, Guid? OrderUnitId, string Value)
    : IAppCommand<Guid>;

public static partial class AddCodeLogger
{
    [LoggerMessage(EventId = 13, Level = LogLevel.Critical, Message = "Add code {Value}")]
    public static partial void Log(ILogger logger, string value);
}

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

public sealed class AddCodeCommandHandler(
    IIntegrationEventOutbox outbox,
    ILogger<AddCodeCommandHandler> logger)
    : IAppCommandHandler<AddCodeCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(AddCodeCommand request, CancellationToken cancellationToken)
    {
        var code = Code.Create(request.OrderId!.Value, request.OrderUnitId!.Value, request.Value);
        if (code.IsFailure)
            return code.Error;

        await outbox.EnqueueAsync(
            CodeAddedIntegrationEvent.Create(
                code.Value.Id,
                code.Value.OrderId,
                code.Value.OrderUnitId,
                code.Value.Value),
            cancellationToken);

        AddCodeLogger.Log(logger, code.Value.Value);

        return code.Value.Id;
    }
}
