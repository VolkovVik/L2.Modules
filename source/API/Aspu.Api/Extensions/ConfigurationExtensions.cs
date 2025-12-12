namespace Aspu.Api.Extensions;

internal static class ConfigurationExtensions
{
    internal static void AddModuleConfiguration(this IConfigurationBuilder configurationBuilder, string[] modules)
    {
        configurationBuilder.AddJsonFile($"serilog.json", false, true);
        configurationBuilder.AddJsonFile($"serilog.Development.json", true, true);

        foreach (var module in modules)
        {
            configurationBuilder.AddJsonFile($"modules.{module}.json", false, true);
            configurationBuilder.AddJsonFile($"modules.{module}.Development.json", true, true);
        }
    }
}
