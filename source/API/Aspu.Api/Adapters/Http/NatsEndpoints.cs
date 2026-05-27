using Aspu.Api.Adapters.Nats;
using Aspu.Common.Presentation.Abstractions.HttpAdapter;
using Microsoft.AspNetCore.Http.HttpResults;
using NATS.Client.Core;

namespace Aspu.Api.Adapters.Http;

internal sealed class NatsEndpoints : IHttpEndpoint
{
    public string Tags => "Nats";

    public void MapEndpoint(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/sensor", static async Task<Results<Ok, BadRequest>> (
            INatsClient? client,
            CancellationToken cancellationToken) =>
        {
            if (client is null)
                return TypedResults.BadRequest();

            var simulator = new SoilSensorSimulator(client, "sensor1", "field1");
            await simulator.RunAsync(cancellationToken);
            return TypedResults.Ok();
        })
        .WithName("SoilSensorSimulator")
        .WithSummary("Soil sensor simulator")
        .WithDescription("Return OK")
        .MapToApiVersion(1);
    }
}
