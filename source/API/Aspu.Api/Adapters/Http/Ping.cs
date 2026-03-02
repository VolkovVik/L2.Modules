using Aspu.Common.Presentation.Endpoints;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Serilog;

namespace Aspu.Api.Adapters.Http;

internal sealed class PingRequest : IEndpoint
{
    public string Tags => "Ping";

    public void MapEndpoint(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/ping", static async Task<Results<Ok<Pong>, NotFound>> (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var response = await mediator.Send(new Ping(Guid.NewGuid()), cancellationToken);
            return response is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(response);
        })
        .WithName("PingRequest")
        .WithSummary("Ping request")
        .WithDescription("Returns pong")
        .MapToApiVersion(1);
    }
}

public sealed record Ping(Guid? Id) : IRequest<Pong?>;

public sealed record Pong(Guid Id);

internal sealed class PingValidator : AbstractValidator<Ping>
{
    public PingValidator()
    {
        RuleFor(x => x.Id).NotNull();
    }
}

public sealed class PingHandler(IMediator _mediator) : IRequestHandler<Ping, Pong?>
{
    public async ValueTask<Pong?> Handle(Ping request, CancellationToken cancellationToken)
    {
        var id = request?.Id ?? Guid.NewGuid();
        Log.Debug("Start {@Id}", id);
        var result = new Pong(id);
        Log.Debug("Publish {@Id}", id);
        await _mediator.Publish(new PingNotification(id), cancellationToken);
        Log.Debug("Stop {@Id}", id);
        return result;
    }
}

public sealed record PingNotification(Guid Id) : INotification;

public sealed class PingNotificationHandler : INotificationHandler<PingNotification>
{
    public ValueTask Handle(PingNotification notification, CancellationToken cancellationToken)
    {
        Log.Debug("Ping notification: {@Id}", notification.Id);
        return ValueTask.CompletedTask;
    }
}
