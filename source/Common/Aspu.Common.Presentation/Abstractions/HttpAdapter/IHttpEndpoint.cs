using Microsoft.AspNetCore.Routing;

namespace Aspu.Common.Presentation.Abstractions.HttpAdapter;

public interface IHttpEndpoint
{
    string Tags { get; }
    void MapEndpoint(IEndpointRouteBuilder routes);
}
