using Microsoft.AspNetCore.Routing;

namespace Aspu.Common.Presentation.Endpoints;

public interface IEndpoint
{
    string Tags { get; }
    void MapEndpoint(IEndpointRouteBuilder routes);
}
