using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Serilog;

namespace Aspu.Api.Adapters.Http;

internal static class PingRequest
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/ping", static async Task<Results<Ok<Pong>, NotFound>> (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var response = await mediator.Send(new Ping(Guid.NewGuid()), cancellationToken);
            return response is not null
                ? TypedResults.Ok(response)
                : TypedResults.NotFound();
        })
        .WithName("PingRequest")
        .WithSummary("Ping request")
        .WithDescription("Returns pong")
        .MapToApiVersion(1)
        .WithTags(Tags.Api);
    }
}

public sealed record Ping(Guid? Id) : IRequest<Pong?>;

public sealed record Pong(Guid Id);

public sealed class PingHandler(IMediator _mediator) : IRequestHandler<Ping, Pong?>
{
    ///public ValueTask<Pong?> Handle(Ping request, CancellationToken cancellationToken) =>
    ///    new(new Pong(request.Id));

    public async ValueTask<Pong?> Handle(Ping request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        Log.Debug("Start {@Id}", id);
        var result = new Pong(request?.Id ?? Guid.NewGuid());

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        Log.Debug("Publish {@Id}", id);
        await _mediator.Publish(new PingNotification(id), cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        Log.Debug("Stop {@Id}", id);
        return result;
    }
}

public sealed class PingValidator : AbstractValidator<Ping>
{
    public PingValidator()
    {
        RuleFor(x => x.Id).NotNull();
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
