using Aspu.Common.Presentation.Endpoints;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Serilog;

namespace Aspu.Modules.Orders.Presentation.Adapters.Http.Codes;

internal sealed class CodeRequest : IEndpoint
{
    public string Tags => "Code";

    public void MapEndpoint(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/Code", static async Task<Results<Ok<CodeDto>, NotFound>> (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var response = await mediator.Send(new Code(Guid.NewGuid()), cancellationToken);
            return response is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(response);
        })
        .WithName("CodeRequest")
        .WithSummary("Code request")
        .WithDescription("Returns CodeDto")
        .MapToApiVersion(1);
    }
}

public sealed record Code(Guid? Id) : IRequest<CodeDto?>;

public sealed record CodeDto(Guid Id);

internal sealed class CodeValidator : AbstractValidator<Code>
{
    public CodeValidator()
    {
        RuleFor(x => x.Id).NotNull();
    }
}

public sealed class CodeHandler(IMediator _mediator) : IRequestHandler<Code, CodeDto?>
{
    public async ValueTask<CodeDto?> Handle(Code request, CancellationToken cancellationToken)
    {
        var id = request.Id!.Value;
        Log.Debug("Start {@Id}", id);
        var result = new CodeDto(id);
        Log.Debug("Publish {@Id}", id);
        await _mediator.Publish(new CodeNotification(id), cancellationToken);
        Log.Debug("Stop {@Id}", id);
        return result;
    }
}

public sealed record CodeNotification(Guid Id) : INotification;

public sealed class CodeNotificationHandler : INotificationHandler<CodeNotification>
{
    public ValueTask Handle(CodeNotification notification, CancellationToken cancellationToken)
    {
        Log.Debug("Code notification: {@Id}", notification.Id);
        return ValueTask.CompletedTask;
    }
}
