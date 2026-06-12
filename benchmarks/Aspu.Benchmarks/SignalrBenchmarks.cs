using Aspu.Api.Ports.Signalr;
using Aspu.Common.Application.Ports.SignalrPort;
using Aspu.Common.SourceGenerators.Application;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

[MemoryDiagnoser]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0047:Declare types in namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S3903:Types should be defined in named namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1110:Declare type inside namespace", Justification = "<Pending>")]
public class SignalrBenchmarks
{
    private const string HubPath = "/notifications-hub";
    private const int MessagesPerIteration = 100;

    private static Context s_ctx = null!;

    public static readonly Test1Notification Notification =
        new("Test description");

    [GlobalSetup]
#pragma warning disable MA0051 // Method is too long
    public static async Task SetupAsync()
#pragma warning restore MA0051 // Method is too long
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
        });

        builder.WebHost.UseTestServer();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        builder.Services
            .AddSignalR()
            .AddJsonProtocol(options =>
                options.PayloadSerializerOptions = new JsonSerializerOptions(SignalrJsonContext.Default.Options));

        builder.Services.AddSingleton<SignalrMetrics>();
        builder.Services.AddSingleton<SignalrMessageWorkerState>();
        builder.Services.AddSingleton<SignalrNotificationChannel>();
        builder.Services.AddSingleton<ISignalrNotificationChannel>(sp => sp.GetRequiredService<SignalrNotificationChannel>());
        builder.Services.AddHostedService<SignalrMessageWorker>();
        builder.Services.AddSingleton<ISignalrNotificationPublisher>(sp =>
        {
            var hubContext = sp.GetRequiredService<IHubContext<SignalrNotificationsHub, ISignalrNotificationsHub>>();
            return SignalrNotificationPublisherRegistration.Create(
                static (host) => SignalrNotificationsHub.GetConnectionId(host),
                () => hubContext.Clients.All,
                (connectionId) => hubContext.Clients.Client(connectionId!),
                (audience) => hubContext.Clients.Group(audience!));
        });

        var app = builder.Build();
        app.MapHub<SignalrNotificationsHub>(HubPath);
        await app.StartAsync();

        var testServer = (TestServer)app.Services.GetRequiredService<IServer>();
        var receiveGate = new ReceiveGate();

        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(testServer.BaseAddress, HubPath), options =>
            {
                options.HttpMessageHandlerFactory = _ => testServer.CreateHandler();
                options.Transports = HttpTransportType.WebSockets;
                options.WebSocketFactory = async (context, cancellationToken) =>
                {
                    var webSocketClient = testServer.CreateWebSocketClient();
                    return await webSocketClient.ConnectAsync(context.Uri, cancellationToken);
                };
            })
            .WithAutomaticReconnect()
            .Build();

        connection.On<Test1Notification>("Test1Notification", _ => receiveGate.Notify());

        await connection.StartAsync();

        var channel = app.Services.GetRequiredService<ISignalrNotificationChannel>();

        receiveGate.Reset();

        await channel.WriteAsync(Notification, CancellationToken.None);
        await receiveGate.WaitAsync(CancellationToken.None);

        s_ctx = new Context(app, connection, channel, receiveGate);
    }

    [GlobalCleanup]
    public static async Task CleanupAsync()
    {
        await s_ctx.Connection.DisposeAsync();
        await s_ctx.App.StopAsync();
        await s_ctx.App.DisposeAsync();
    }

    [IterationSetup]
    public void ResetReceiveGate() => s_ctx.ReceiveGate.Reset();

    /// <summary>
    /// Publish on server and wait until the WebSocket client receives Test1Notification.
    /// Reported Mean/Allocated are per single message (OperationsPerInvoke = 100).
    /// </summary>
    [Benchmark(OperationsPerInvoke = MessagesPerIteration)]
    public async Task SendToWebSocketClient()
    {
        for (var i = 0; i < MessagesPerIteration; i++)
        {
            s_ctx.ReceiveGate.Reset();
            await s_ctx.Channel.WriteAsync(Notification, CancellationToken.None);
            await s_ctx.ReceiveGate.WaitAsync(CancellationToken.None);
        }
    }

    private sealed class ReceiveGate
    {
        private TaskCompletionSource _completion = CreateCompletion();

        public void Reset() => _completion = CreateCompletion();

        public void Notify() => _completion.TrySetResult();

        public Task WaitAsync(CancellationToken cancellationToken) =>
            _completion.Task.WaitAsync(cancellationToken);

        private static TaskCompletionSource CreateCompletion() =>
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class Context(
        WebApplication app,
        HubConnection connection,
        ISignalrNotificationChannel channel,
        ReceiveGate receiveGate)
    {
        public WebApplication App { get; } = app;

        public HubConnection Connection { get; } = connection;

        public ISignalrNotificationChannel Channel { get; } = channel;

        public ReceiveGate ReceiveGate { get; } = receiveGate;
    }
}
