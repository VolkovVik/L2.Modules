using System.Reflection;
using Aspu.Api.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

int[] versions = [1, 2];
builder.Services.AddOpenApi(versions);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(versions);
}

app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
    return $"Hello from ASPU.API version: {version}";
})
.WithTags("Api")
.WithName("Version");

await app.RunAsync();
