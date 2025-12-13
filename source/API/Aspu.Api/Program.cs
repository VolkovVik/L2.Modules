using System.Reflection;
using Aspu.Api.Extensions;
using Aspu.Api.Extensions.Exceptions;
using Aspu.Api.Extensions.HttpLogging;
using Scalar.AspNetCore;
using Serilog;

SerilogExtensions.AddDefaultLogging();

Log.Information("Starting ASPU API application");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddModuleConfiguration([]);

    builder.Services.AddLogging(builder.Configuration);

    builder.AddHttpRequestLogging();

    builder.Services.AddExceptionHandlers();

    int[] versions = [1, 2];
    builder.Services.AddOpenApi(versions);

    var app = builder.Build();

    app.UseHttpLogging();

    app.UseExceptionHandler();

    app.UseStatusCodePages();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(versions);
    }

    app.UseHttpsRedirection();

    app.MapGet("/", () => "Hello from ASPU.API")
        .WithTags("Api")
        .WithName("Version")
        .WithDescription("Returns API version");

    var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
    Log.Information("Running ASPU API application: {@Version}", version);

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ASPU API application terminated unexpectedly");
}
finally
{
    Log.Information("Stopping ASPU API application");
    await Log.CloseAndFlushAsync();
}

public partial class Program;
