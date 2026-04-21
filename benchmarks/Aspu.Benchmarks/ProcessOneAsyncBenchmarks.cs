using Aspu.Api.Adapters.Mqtt;
using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.Mqtt;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

[MemoryDiagnoser]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0047:Declare types in namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S3903:Types should be defined in named namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1110:Declare type inside namespace", Justification = "<Pending>")]
public class ProcessOneAsyncBenchmarks
{
    private static Context s_ctx = null!;

    public const string TopicName = "benchmark/topic";

    [GlobalSetup]
    public static void Setup()
    {
        var mqttOptions = Options.Create(new MqttOptions());
        var queue = new MqttInboundMessageQueue(mqttOptions);

        var services = new ServiceCollection();
        services.AddSingleton<IMqttMessageHandler, BenchmarkMqttHandler>();
        services.AddSingleton<MqttMessageHandlerTopicRegistry>();
        var rootServices = services.BuildServiceProvider();

        s_ctx = new Context
        {
            RootServices = rootServices,
            Service = new MqttInboundMessageHostedService(
                queue,
                rootServices.GetRequiredService<IServiceScopeFactory>(),
                rootServices.GetRequiredService<MqttMessageHandlerTopicRegistry>(),
                mqttOptions,
                NullLogger<MqttInboundMessageHostedService>.Instance),
            MatchedTopic = new MqttInboundMessage
            {
                Topic = TopicName,
                Payload = new byte[128],
            },
        };
    }

    [Benchmark(Baseline = true)]
    public Task MatchedHandler() =>
        s_ctx.Service.ProcessOneAsync(s_ctx.MatchedTopic, CancellationToken.None);

    private sealed class Context
    {
        public required ServiceProvider RootServices { get; init; }

        public required MqttInboundMessageHostedService Service { get; init; }

        public required MqttInboundMessage MatchedTopic { get; init; }
    }

    private sealed class BenchmarkMqttHandler : IMqttMessageHandler
    {
        public string Topic => TopicName;

        public Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
