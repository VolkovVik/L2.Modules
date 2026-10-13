namespace Aspu.Api.Extensions;

internal static class ConfigurationExtensions
{
    internal static void AddModuleConfiguration(
        this IConfigurationBuilder configurationBuilder,
        IHostEnvironment environment,
        string[] modules)
    {
        // Environment-specific files are loaded only for the current environment, like appsettings.{Environment}.json.
        var environmentName = environment.EnvironmentName;

        configurationBuilder.AddJsonFile("serilog.json", optional: false, reloadOnChange: true);
        configurationBuilder.AddJsonFile($"serilog.{environmentName}.json", optional: true, reloadOnChange: true);

        foreach (var module in modules)
        {
            configurationBuilder.AddJsonFile($"modules.{module}.json", optional: false, reloadOnChange: true);
            configurationBuilder.AddJsonFile($"modules.{module}.{environmentName}.json", optional: true, reloadOnChange: true);
        }
    }
}
