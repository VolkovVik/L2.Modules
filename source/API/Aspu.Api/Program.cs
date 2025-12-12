using System.Reflection;
using Aspu.Api.Extensions;
using Scalar.AspNetCore;
using Serilog;

SerilogExtensions.AddDefaultConfiguration();

Log.Information("Starting ASPU API application");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddModuleConfiguration([]);

    builder.Services.AddSerilog((provider, loggerConfiguration) =>
        loggerConfiguration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(provider)
            .Enrich.FromLogContext());

    builder.Services.AddHttpLogging(options => { });

    int[] versions = [1, 2];
    builder.Services.AddOpenApi(versions);

    var app = builder.Build();

    app.UseHttpLogging();

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
