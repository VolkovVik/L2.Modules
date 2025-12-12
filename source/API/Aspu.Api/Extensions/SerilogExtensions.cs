using System.Globalization;
using Serilog;

namespace Aspu.Api.Extensions;

internal static class SerilogExtensions
{
    internal static void AddDefaultConfiguration()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();
    }
}
