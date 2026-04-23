using Aspu.Common.Presentation.Abstractions.HttpAdapter;
using Aspu.Modules.Orders.Application.UseCases.Codes.Commands.AddCode;
using Aspu.Modules.Orders.Application.UseCases.Codes.Commands.GetCode;
using Aspu.Modules.Orders.Application.UseCases.Codes.Commands.GetCodeById;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Aspu.Modules.Orders.Presentation.Adapters.Http.Codes;

internal sealed class CodeRequests : IHttpEndpoint
{
    public string Tags => "Code";

    public void MapEndpoint(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/{id}", static async Task<Results<Ok<Guid>, NotFound>> (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var response = await mediator.Send(new GetCodeByIdCommand(id), cancellationToken);
            return response?.IsSuccess is not true
                ? TypedResults.NotFound()
                : TypedResults.Ok(response.Value);
        })
        .WithName("GetCodesById")
        .WithSummary("Get codes by ID")
        .WithDescription("Returns dto")
        .MapToApiVersion(1);

        routes.MapGet("/code/{value}", static async Task<Results<Ok<Guid>, NotFound>> (
            string value,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var response = await mediator.Send(new GetCodeCommand(value), cancellationToken);
            return response?.IsSuccess is not true
                ? TypedResults.NotFound()
                : TypedResults.Ok(response.Value);
        })
        .WithName("GetCodesByValue")
        .WithSummary("Get codes by value")
        .WithDescription("Returns dto")
        .MapToApiVersion(1);

        routes.MapPost("/add", static async Task<Results<Ok<Guid>, NotFound>> (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var response = await mediator.Send(new AddCodeCommand(Guid.NewGuid(), Guid.NewGuid(), "test"), cancellationToken);
            return response?.IsSuccess is not true
                ? TypedResults.NotFound()
                : TypedResults.Ok(response.Value);
        })
        .WithName("AddCodes")
        .WithSummary("Add codes")
        .WithDescription("Returns result")
        .MapToApiVersion(1);
    }
}
