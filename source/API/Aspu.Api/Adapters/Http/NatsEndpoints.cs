using System.Globalization;
using Aspu.Common.Presentation.Abstractions.HttpAdapter;
using Microsoft.AspNetCore.Http.HttpResults;
using NATS.Client.Core;

namespace Aspu.Api.Adapters.Http;

internal sealed class NatsEndpoints : IHttpEndpoint
{
    public string Tags => "Nats";

    public void MapEndpoint(IEndpointRouteBuilder routes)
    {
        // GET /sensor?sensorId=123&fieldId=456
        routes.MapGet("/sensor", static async Task<Results<Ok, BadRequest>> (
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
            .WithName("SendNatsMessage1")
            .WithSummary("Send nats message")
            .WithDescription("Return OK")
            .MapToApiVersion(1);

        // GET /sensor/{sensorId}/{fieldId}
        routes.MapGet("/sensor/{sensorId:int}/{fieldId:int}", static async Task<Results<Ok, BadRequest>> (
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
            .WithName("SendNatsMessage2")
            .WithSummary("Send nats message")
            .WithDescription("Return OK")
            .MapToApiVersion(2);
    }
}
