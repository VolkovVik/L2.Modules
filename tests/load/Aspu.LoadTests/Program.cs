using NBomber.CSharp;
using NBomber.Http;
using NBomber.Http.CSharp;
using NBomber.Plugins.Network.Ping;

// dotnet build -c Release
// dotnet run -c Release

using var httpClient = Http.CreateDefaultClient();

var scenario = Scenario.Create("http_scenario", async context =>
{
    var request = Http.CreateRequest("GET", "https://localhost:5001/")
        .WithHeader("Content-Type", "application/json");
    /// .WithBody(new StringContent("{ some JSON }", Encoding.UTF8, "application/json"));

    var response = await Http.Send(httpClient, request);
    return response;
})
.WithWarmUpDuration(TimeSpan.FromSeconds(10))
.WithLoadSimulations(
    //Simulation.RampingConstant(copies: 10, during: TimeSpan.FromSeconds(30)),
    Simulation.KeepConstant(copies: 100, during: TimeSpan.FromSeconds(30))
//Simulation.RampingConstant(copies: 0, during: TimeSpan.FromSeconds(30))
);

NBomberRunner
    .RegisterScenarios(scenario)
    .WithWorkerPlugins(
        new PingPlugin(PingPluginConfig.CreateDefault("nbomber.com")),
        new HttpMetricsPlugin([HttpVersion.Version1])
    )
    .Run();
