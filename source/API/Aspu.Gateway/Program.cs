using Aspu.Gateway.Extensions;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddGatewayReverseProxy(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();

/// app.UseHttpsRedirection();

app.UseGatewayReverseProxy();

await app.RunAsync();
