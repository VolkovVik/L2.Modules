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
            return response is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(response);
        })
        .WithName("PingRequest1")
        .WithSummary("Ping request1")
        .WithDescription("Returns pong1")
        .MapToApiVersion(1);
    }
}

internal sealed class Ping2Request : IEndpoint
{
    public string Tags => "Ping2";

    public void MapEndpoint(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/ping2", static async Task<Results<Ok<Pong>, NotFound>> (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var response = await mediator.Send(new Ping(Guid.NewGuid()), cancellationToken);
            return response is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(response);
        })
        .WithName("PingRequest2")
        .WithSummary("Ping request")
        .WithDescription("Returns pong")
        .MapToApiVersion(1);
    }
}

internal sealed class Ping3Request : IEndpoint
{
    public string Tags => "Ping3";

    public void MapEndpoint(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/ping3", static async Task<Results<Ok<Pong>, NotFound>> (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var response = await mediator.Send(new Ping(Guid.NewGuid()), cancellationToken);
            return response is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(response);
        })
        .WithName("PingRequest3")
        .WithSummary("Ping request 3")
        .WithDescription("Returns pong 3")
        .MapToApiVersion(1);
    }
}

internal sealed class Ping4Request : IEndpoint
{
    public string Tags => "Ping4";

    public void MapEndpoint(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/ping4", static async Task<Results<Ok<Pong>, NotFound>> (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var response = await mediator.Send(new Ping(Guid.NewGuid()), cancellationToken);
            return response is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(response);
        })
        .WithName("PingRequest4")
        .WithSummary("Ping request 4")
        .WithDescription("Returns pong 4")
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

public sealed record PingNotification(Guid Id) : INotification;

public sealed class PingNotificationHandler : INotificationHandler<PingNotification>
{
    public ValueTask Handle(PingNotification notification, CancellationToken cancellationToken)
    {
        Log.Debug("Ping notification: {@Id}", notification.Id);
        return ValueTask.CompletedTask;
    }
}
