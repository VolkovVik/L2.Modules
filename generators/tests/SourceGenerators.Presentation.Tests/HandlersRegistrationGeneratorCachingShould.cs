using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceGenerators.Presentation.Tests;

/// <summary>
/// The generator is incremental: the registration source is regenerated only when a handler changes,
/// not on every edit of the project.
/// </summary>
internal sealed class HandlersRegistrationGeneratorCachingShould
{
    private const string Handler = """
        using Aspu.Common.Presentation.Abstractions.NatsAdapter;
        namespace Demo { internal sealed class OrdersHandler : INatsHandler { public string Topic => "orders.*"; } }
        """;

    [Test]
    public async Task Reuse_Output_When_Unrelated_File_Changes()
    {
        var compilation = GeneratorRunner.CreateCompilation(Handler);
        var driver = GeneratorRunner.CreateTrackingDriver(new NatsHandlersRegistrationGenerator()).RunGenerators(compilation);

        driver = driver.RunGenerators(compilation.AddSyntaxTrees(
            CSharpSyntaxTree.ParseText("namespace Demo { internal static class Unrelated { public static int Value => 1; } }")));

        await Assert.That(driver.OutputReason()).IsEqualTo(IncrementalStepRunReason.Cached);
    }

    [Test]
    public async Task Regenerate_When_Handler_Topic_Changes()
    {
        var compilation = GeneratorRunner.CreateCompilation(Handler);
        var driver = GeneratorRunner.CreateTrackingDriver(new NatsHandlersRegistrationGenerator()).RunGenerators(compilation);

        var handlerTree = compilation.SyntaxTrees.Single(t => string.Equals(t.FilePath, GeneratorRunner.HandlersFile, StringComparison.Ordinal));
        driver = driver.RunGenerators(compilation.ReplaceSyntaxTree(
            handlerTree,
            CSharpSyntaxTree.ParseText(Handler.Replace("orders.*", "orders.>", StringComparison.Ordinal), path: GeneratorRunner.HandlersFile)));

        await Assert.That(driver.OutputReason()).IsEqualTo(IncrementalStepRunReason.Modified);
        await Assert.That(driver.GetRunResult().Results.Single().EmittedTopicLiterals()).IsEquivalentTo(["\"orders.>\""]);
    }
}
