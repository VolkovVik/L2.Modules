namespace Aspu.Api.Extensions;

internal static class ConfigurationExtensions
{
    internal static void AddModuleConfiguration(this IConfigurationBuilder configurationBuilder, string[] modules)
    {
        configurationBuilder.AddJsonFile($"serilog.json", optional: false, reloadOnChange: true);
        configurationBuilder.AddJsonFile($"serilog.Development.json", optional: true, reloadOnChange: true);

        foreach (var module in modules)
        {
            configurationBuilder.AddJsonFile($"modules.{module}.json", optional: false, reloadOnChange: true);
            configurationBuilder.AddJsonFile($"modules.{module}.Development.json", optional: true, reloadOnChange: true);
        }
    }
}
