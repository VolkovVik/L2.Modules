using System.Globalization;
using Aspu.Common.Presentation.Abstractions.HttpAdapter;
using Microsoft.AspNetCore.Http.HttpResults;
using NATS.Client.Core;
using NATS.Client.JetStream;

namespace Aspu.Api.Adapters.Http;

internal sealed class NatsEndpoints : IHttpEndpoint
{
    public string Tags => "Nats";

#pragma warning disable MA0051 // Method is too long
    public void MapEndpoint(IEndpointRouteBuilder routes)
#pragma warning restore MA0051 // Method is too long
    {
        routes.MapGet("/ping", static async Task<Results<Ok<string>, BadRequest>> (
            INatsClient? client,
            CancellationToken cancellationToken) =>
        {
            if (client is null)
                return TypedResults.BadRequest();

            var timespan = await client.PingAsync(cancellationToken: cancellationToken);
            return TypedResults.Ok(string.Create(CultureInfo.InvariantCulture, $"{timespan.TotalMicroseconds} mks"));
        })
            .WithName("NatsPingRequest")
            .WithSummary("Nats ping request")
            .WithDescription("Return ping time")
            .MapToApiVersion(1);

        // GET /sensor?sensorId=123&fieldId=456
        routes.MapGet("/test", static async Task<Results<Ok, BadRequest>> (
            int sensorId1,
            int fieldId,
            INatsClient? client,
            CancellationToken cancellationToken) =>
        {
            if (client is null)
                return TypedResults.BadRequest();

            var payload = new
            {
                TimestampUtc = DateTime.UtcNow,
                FieldId = string.Create(CultureInfo.InvariantCulture, $"field{fieldId}"),
                SensorId = string.Create(CultureInfo.InvariantCulture, $"sensor{sensorId1}"),
            };
            await client.PublishAsync("test.message", payload, cancellationToken: cancellationToken);
            return TypedResults.Ok();
        })
            .WithName("NatsTestMessage1")
            .WithSummary("Nats test message")
            .WithDescription("Return test response")
            .MapToApiVersion(1);

        // GET /sensor/{sensorId}/{fieldId}
        routes.MapGet("/test/{sensorId:int}/{fieldId:int}", static async Task<Results<Ok, BadRequest>> (
            int sensorId,
            int fieldId,
            INatsClient? client,
            CancellationToken cancellationToken) =>
        {
            if (client is null)
                return TypedResults.BadRequest();

            var payload = new
            {
                TimestampUtc = DateTime.UtcNow,
                FieldId = string.Create(CultureInfo.InvariantCulture, $"field{fieldId}"),
                SensorId = string.Create(CultureInfo.InvariantCulture, $"sensor{sensorId}"),
            };

            await client.PublishAsync("test.message", payload, cancellationToken: cancellationToken);
            return TypedResults.Ok();
        })
            .WithName("NatsTestMessage2")
            .WithSummary("Nats test message")
            .WithDescription("Return test response")
            .MapToApiVersion(1);


        routes.MapPost("/jetstream", static async (CreateJob request, INatsJSContext js, CancellationToken cancellationToken) =>
        {
            var payload = new
            {
                Id = Guid.NewGuid(),
                TimestampUtc = DateTime.UtcNow,
                request.Payload,
            };

            var ack = await js.PublishAsync("test.message", payload, cancellationToken: cancellationToken);
            ack.EnsureSuccess();

            return Results.Accepted($"/jetstream/{payload.Id}");
        })
            .WithName("NatsJetStreamTestMessage")
            .WithSummary("Nats jetstream test message")
            .WithDescription("Return job ID")
            .MapToApiVersion(1);
    }
}

internal sealed record CreateJob(string Payload);
