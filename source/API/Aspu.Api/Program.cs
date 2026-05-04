using System.Reflection;
using Aspu.Api.Extensions;
using Aspu.Api.Extensions.Exceptions;
using Aspu.Api.Extensions.HttpLogging;
using Serilog;

SerilogExtensions.AddDefaultLogging();

Log.Information("Starting ASPU API application");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddModuleConfiguration([]);

    builder.Services.AddOptions(builder.Configuration);

    builder.Services.AddLogging(builder.Configuration);

    builder.AddHttpRequestLogging();

    builder.Services.AddExceptionHandlers();

    builder.Services.AddEndpointExtension()
        .AddOpenApiExtension();

    builder.Services.AddObjectPoolExtension();

    builder.Services.AddMediatorExtension();

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddModules(builder.Configuration);

    var app = builder.Build();

    app.UseHttpLogging();

    app.UseExceptionHandler();

    app.UseStatusCodePages();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApiExtension();
        app.MapScalarExtension();
    }

    /// app.UseHttpsRedirection();

    app.UseEndpointExtension();

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

#pragma warning disable S1118
#pragma warning disable ASP0027 // Unnecessary public Program class declaration
public partial class Program;
#pragma warning restore ASP0027 // Unnecessary public Program class declaration
#pragma warning restore S1118
