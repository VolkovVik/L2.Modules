using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceGenerators.Presentation.Tests;

/// <summary>
/// Compiles handler source together with stub transport interfaces and runs a handlers registration generator on it.
/// Stubs mirror the real interfaces' names and namespaces, which is all the generator looks at.
/// </summary>
internal static class GeneratorRunner
{
    public const string HandlersFile = "Handlers.cs";

    private const string Contracts = """
        namespace Aspu.Common.Presentation.Abstractions.NatsAdapter
        {
            public interface INatsHandler { string Topic { get; } }
        }

        namespace Aspu.Common.Presentation.Abstractions.MqttAdapter
        {
            public interface IMqttHandler { string Topic { get; } }
        }
        """;

    // The test host runs on the same framework, so its trusted platform assemblies are a complete reference set.
    private static readonly ImmutableArray<MetadataReference> References =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path)),
    ];

    public static CSharpCompilation CreateCompilation(string handlers) =>
        CSharpCompilation.Create(
            "Demo.Presentation",
            [CSharpSyntaxTree.ParseText(Contracts, path: "Contracts.cs"), CSharpSyntaxTree.ParseText(handlers, path: HandlersFile)],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    public static GeneratorRunResult Run(IIncrementalGenerator generator, string handlers) =>
        CSharpGeneratorDriver.Create(generator)
            .RunGenerators(CreateCompilation(handlers))
            .GetRunResult()
            .Results
            .Single();

    public static GeneratorDriver CreateTrackingDriver(IIncrementalGenerator generator) =>
        CSharpGeneratorDriver.Create(
            [generator.AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

    public static string GeneratedText(this GeneratorRunResult result) =>
        string.Concat(result.GeneratedSources.Select(s => s.SourceText.ToString()));

    /// <summary>Topic arguments of the emitted <c>InboundProcessorTopic</c> registrations, as written in the generated code.</summary>
    public static IReadOnlyList<string> EmittedTopicLiterals(this GeneratorRunResult result) =>
    [
        .. result.GeneratedSources
            .SelectMany(s => s.SourceText.Lines)
            .Select(l => l.ToString())
            .Where(l => l.Contains("new InboundProcessorTopic<", StringComparison.Ordinal))
            .Select(l => l[(l.IndexOf(">(", StringComparison.Ordinal) + 2)..l.IndexOf(", typeof(", StringComparison.Ordinal)]),
    ];

    public static IReadOnlyList<string> DiagnosticIds(this GeneratorRunResult result) =>
        [.. result.Diagnostics.Select(d => d.Id)];

    public static IncrementalStepRunReason OutputReason(this GeneratorDriver driver) =>
        driver.GetRunResult().Results.Single().TrackedOutputSteps
            .SelectMany(step => step.Value)
            .SelectMany(step => step.Outputs)
            .Single()
            .Reason;
}
