using System.Reflection;
using BenchmarkDotNet.Running;

/// <summary>
/// dotnet run -c Release --project benchmarks/Aspu.Benchmarks/Aspu.Benchmarks.csproj -- --filter *MqttBenchmarks*
/// dotnet run -c Release --project benchmarks/Aspu.Benchmarks/Aspu.Benchmarks.csproj -- --filter *SignalrBenchmarks*
/// </summary>

if (args.Length > 0)
{
    BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
}
else
{
    /// BenchmarkRunner.Run<LogFormatterTests>();
    /// BenchmarkRunner.Run<StringBuilderTests>();
    BenchmarkRunner.Run<ParsingCodesTests>();
}
