using Microsoft.AspNetCore.Routing;

namespace Aspu.Common.Presentation.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
