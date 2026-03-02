using Aspu.Common.Presentation.Endpoints;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Serilog;

namespace Aspu.Api.Adapters.Http;

internal sealed class Ping1Request : IEndpoint
{
    public string Tags => "Ping1";

    public void MapEndpoint(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/ping1", static async Task<Results<Ok<Pong>, NotFound>> (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var response = await mediator.Send(new Ping(Guid.NewGuid()), cancellationToken);
            return response is not null
                ? TypedResults.Ok(response)
                : TypedResults.NotFound();
        })
        .WithName("PingRequest1")
        .WithSummary("Ping request 1")
        .WithDescription("Returns pong 1")
        .MapToApiVersion(1);
    }
}

public sealed record Ping1(Guid? Id) : IRequest<Pong1?>;

public sealed record Pong1(Guid Id);

public sealed class Ping1Handler(IMediator _mediator) : IRequestHandler<Ping1, Pong1?>
{
    ///public ValueTask<Pong?> Handle(Ping request, CancellationToken cancellationToken) =>
    ///    new(new Pong(request.Id));

    public async ValueTask<Pong1?> Handle(Ping1 request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        Log.Debug("Start {@Id}", id);
        var result = new Pong1(request?.Id ?? Guid.NewGuid());

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        Log.Debug("Publish {@Id}", id);
        await _mediator.Publish(new Ping1Notification(id), cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        Log.Debug("Stop {@Id}", id);
        return result;
    }
}

public sealed class Ping1Validator : AbstractValidator<Ping1>
{
    public Ping1Validator()
    {
        RuleFor(x => x.Id).NotNull();
    }
}

public sealed record Ping1Notification(Guid Id) : INotification;

public sealed class Ping1NotificationHandler : INotificationHandler<Ping1Notification>
{
    public ValueTask Handle(Ping1Notification notification, CancellationToken cancellationToken)
    {
        Log.Debug("Ping notification: {@Id}", notification.Id);
        return ValueTask.CompletedTask;
    }
}
