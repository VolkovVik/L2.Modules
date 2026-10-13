namespace SourceGenerators.Presentation.Tests;

internal sealed class HandlersRegistrationGeneratorShould
{
    [Test]
    public async Task Register_Handler_Keyed_By_Type_With_Its_Topic()
    {
        var result = GeneratorRunner.Run(new NatsHandlersRegistrationGenerator(), NatsHandler("public string Topic => \"orders.*\";"));

        var generated = result.GeneratedText();
        await Assert.That(result.DiagnosticIds()).IsEmpty();
        await Assert.That(generated).Contains("public static IServiceCollection AddNatsHandlers(", StringComparison.Ordinal);
        await Assert.That(generated).Contains("services.TryAddKeyedScoped<INatsHandler, global::Demo.OrdersHandler>(typeof(global::Demo.OrdersHandler));", StringComparison.Ordinal);
        await Assert.That(generated).Contains("services.AddSingleton(new InboundProcessorTopic<INatsHandler>(\"orders.*\", typeof(global::Demo.OrdersHandler)));", StringComparison.Ordinal);
    }

    [Test]
    public async Task Use_Transport_Name_For_Mqtt()
    {
        const string handlers = """
            using Aspu.Common.Presentation.Abstractions.MqttAdapter;
            namespace Demo { internal sealed class OrdersHandler : IMqttHandler { public string Topic => "orders/+"; } }
            """;

        var result = GeneratorRunner.Run(new MqttHandlersRegistrationGenerator(), handlers);

        await Assert.That(result.GeneratedText()).Contains("public static IServiceCollection AddMqttHandlers(", StringComparison.Ordinal);
        await Assert.That(result.EmittedTopicLiterals()).IsEquivalentTo(["\"orders/+\""]);
    }

    [Test]
    [Arguments("public string Topic => \"orders.*\";", "\"orders.*\"")]
    [Arguments("public string Topic { get => \"orders.*\"; }", "\"orders.*\"")]
    [Arguments("public string Topic { get { return \"orders.*\"; } }", "\"orders.*\"")]
    [Arguments("public string Topic { get; } = \"orders.*\";", "\"orders.*\"")]
    [Arguments("public string Topic => Topics.Orders;", "\"orders.*\"")]
    [Arguments("public string Topic => \"orders\" + \".*\";", "\"orders.*\"")]
    [Arguments("public string Topic => \"\"\"orders.raw\"\"\";", "\"orders.raw\"")]
    [Arguments("public string Topic => \"  orders.*  \";", "\"orders.*\"")]
    [Arguments("public string Topic => @\"a\\b\";", "\"a\\\\b\"")]
    public async Task Emit_Constant_Topic(string topicMember, string expectedLiteral)
    {
        var result = GeneratorRunner.Run(new NatsHandlersRegistrationGenerator(), NatsHandler(topicMember));

        await Assert.That(result.DiagnosticIds()).IsEmpty();
        await Assert.That(result.EmittedTopicLiterals()).IsEquivalentTo([expectedLiteral]);
    }

    [Test]
    public async Task Take_Topic_From_Base_Class()
    {
        const string handlers = """
            using Aspu.Common.Presentation.Abstractions.NatsAdapter;
            namespace Demo
            {
                internal abstract class HandlerBase : INatsHandler { public string Topic => "base.topic"; }
                internal sealed class OrdersHandler : HandlerBase { }
            }
            """;

        var result = GeneratorRunner.Run(new NatsHandlersRegistrationGenerator(), handlers);

        // The abstract base itself isn't a handler, so only the derived class is registered.
        await Assert.That(result.DiagnosticIds()).IsEmpty();
        await Assert.That(result.EmittedTopicLiterals()).IsEquivalentTo(["\"base.topic\""]);
    }

    [Test]
    public async Task Register_Record_Handler()
    {
        const string handlers = """
            using Aspu.Common.Presentation.Abstractions.NatsAdapter;
            namespace Demo { internal sealed record OrdersHandler(int Value) : INatsHandler { public string Topic => "orders.*"; } }
            """;

        var result = GeneratorRunner.Run(new NatsHandlersRegistrationGenerator(), handlers);

        await Assert.That(result.DiagnosticIds()).IsEmpty();
        await Assert.That(result.EmittedTopicLiterals()).IsEquivalentTo(["\"orders.*\""]);
    }

    [Test]
    public async Task Not_Generate_Source_Without_Handlers()
    {
        var result = GeneratorRunner.Run(new NatsHandlersRegistrationGenerator(), "namespace Demo { internal sealed class NotAHandler { } }");

        await Assert.That(result.GeneratedSources).IsEmpty();
        await Assert.That(result.DiagnosticIds()).IsEmpty();
    }

    [Test]
    [Arguments("public string Topic => System.Environment.MachineName;")]
    [Arguments("#nullable disable\n    public string Topic => null;")]
    public Task Report_Non_Constant_Topic(string topicMember) =>
        AssertSingleDiagnosticAsync(NatsHandler(topicMember), "ASPU001");

    [Test]
    [Arguments("internal class OrdersHandler : INatsHandler")]
    [Arguments("internal static class Outer { private sealed class OrdersHandler : INatsHandler")]
    public Task Report_Not_Registrable_Handler(string declaration)
    {
        var closing = declaration.Contains("Outer", StringComparison.Ordinal) ? "} }" : "}";
        var handlers = $$"""
            using Aspu.Common.Presentation.Abstractions.NatsAdapter;
            namespace Demo { {{declaration}} { public string Topic => "orders.*"; {{closing}} }
            """;

        return AssertSingleDiagnosticAsync(handlers, "ASPU002");
    }

    [Test]
    public Task Report_Empty_Topic() =>
        AssertSingleDiagnosticAsync(NatsHandler("public string Topic => \"   \";"), "ASPU003");

    [Test]
    [Arguments("internal sealed class OrdersHandler<T> : INatsHandler { public string Topic => \"orders.*\"; }")]
    [Arguments("internal static class Outer<T> { internal sealed class OrdersHandler : INatsHandler { public string Topic => \"orders.*\"; } }")]
    public Task Report_Generic_Handler(string declaration) =>
        AssertSingleDiagnosticAsync($"using Aspu.Common.Presentation.Abstractions.NatsAdapter; namespace Demo {{ {declaration} }}", "ASPU004");

    [Test]
    public async Task Report_Duplicate_Topic_On_Each_Handler_And_Keep_Unique_Ones()
    {
        const string handlers = """
            using Aspu.Common.Presentation.Abstractions.NatsAdapter;
            namespace Demo
            {
                internal sealed class OrdersCreated : INatsHandler { public string Topic => "orders.created"; }
                internal sealed class OrdersCreatedCopy : INatsHandler { public string Topic => " orders.created "; }
                internal sealed class Invoices : INatsHandler { public string Topic => "invoices.>"; }
            }
            """;

        var result = GeneratorRunner.Run(new NatsHandlersRegistrationGenerator(), handlers);

        await Assert.That(result.DiagnosticIds()).IsEquivalentTo(["ASPU005", "ASPU005"]);
        await Assert.That(result.EmittedTopicLiterals()).IsEquivalentTo(["\"invoices.>\""]);
    }

    private static async Task AssertSingleDiagnosticAsync(string handlers, string expectedId)
    {
        var result = GeneratorRunner.Run(new NatsHandlersRegistrationGenerator(), handlers);

        await Assert.That(result.DiagnosticIds()).IsEquivalentTo([expectedId]);
        // Reported on the handler, not on the generated file, and the handler isn't registered.
        await Assert.That(result.Diagnostics.Single().Location.GetLineSpan().Path).IsEqualTo(GeneratorRunner.HandlersFile);
        await Assert.That(result.EmittedTopicLiterals()).IsEmpty();
    }

    private static string NatsHandler(string topicMember) => $$"""
        using Aspu.Common.Presentation.Abstractions.NatsAdapter;
        namespace Demo
        {
            internal static class Topics { public const string Orders = "orders.*"; }

            internal sealed class OrdersHandler : INatsHandler
            {
                {{topicMember}}
            }
        }
        """;
}
