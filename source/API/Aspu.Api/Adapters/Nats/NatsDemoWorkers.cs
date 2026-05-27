using NATS.Client.Core;
using Serilog;

namespace Aspu.Api.Adapters.Nats;

public interface INatsInboundMessage
{
    string FieldId { get; }
    DateTime TimestampUtc { get; }
}

public record SoilMoistureReading(
    string SensorId,
    string FieldId,
    double MoisturePercent,
    DateTime TimestampUtc) : INatsInboundMessage;

public record IrrigationCommand(
    string FieldId,
    bool EnableIrrigation,
    double TargetMoisturePercent,
    DateTime TimestampUtc) : INatsInboundMessage;

public sealed class SoilSensorSimulator(INatsClient client, string sensorId, string fieldId)
{
    private readonly Random _random = new();

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var reading = new SoilMoistureReading(
            SensorId: sensorId,
            FieldId: fieldId,
            MoisturePercent: 15 + _random.NextDouble() * 20,
            TimestampUtc: DateTime.UtcNow);

        var subject = $"sensors.soil.moisture.{fieldId}";
        await client.PublishAsync(subject, reading, cancellationToken: cancellationToken);
        await Task.Delay(5, cancellationToken);
    }
}

public sealed class SoilAnalyticsWorker(INatsClient client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var msg in client.SubscribeAsync<SoilMoistureReading>(
            subject: "sensors.soil.moisture.*",
            cancellationToken: stoppingToken))
        {
            Log.Information(
                "[Analytics] Field={@FieldId}, Sensor={@SensorId}, Moisture={@MoisturePercent:F1}%",
                msg.Data?.FieldId,
                msg.Data?.SensorId,
                msg.Data?.MoisturePercent);
        }
    }
}

public sealed class SoilAnalyticsWorker1(INatsClient client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var msg in client.SubscribeAsync<string>(
            subject: "sensors.soil.moisture.*",
            cancellationToken: stoppingToken))
        {
            /// var reading = msg.Data;
            Log.Information("[Analytics1] Subject={@Subject}  Value= {@Value}", msg.Subject, msg.Data);
        }
    }
}

public sealed class SoilAnalyticsWorker2(INatsClient client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var msg in client.SubscribeAsync<string>(
            subject: "sensors.soil.*.*",
            cancellationToken: stoppingToken))
        {
            /// var reading = msg.Data;
            Log.Information("[Analytics2] Subject={@Subject}  Value= {@Value}", msg.Subject, msg.Data);
        }
    }
}

public sealed class SoilAnalyticsWorker3(INatsClient client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var msg in client.SubscribeAsync<string>(
            subject: ">",
            cancellationToken: stoppingToken))
        {
            /// var reading = msg.Data;
            Log.Information("[Analytics3] Subject={@Subject}  Value= {@Value}", msg.Subject, msg.Data);
        }
    }
}

public sealed class SoilAnalyticsWorker4(INatsClient client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var msg in client.SubscribeAsync<byte[]>(
            subject: ">",
            cancellationToken: stoppingToken))
        {
            /// var reading = msg.Data;
            Log.Information("[Analytics4] Subject={@Subject}  Value= {@Value}", msg.Subject, msg.Data?.Length);
        }
    }
}

public sealed class IrrigationDeviceWorker(INatsClient client) : BackgroundService
{
    private const string DeviceId = "device-1";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var msg in client.SubscribeAsync<IrrigationCommand>(
            subject: "irrigation.commands",
            cancellationToken: stoppingToken))
        {
            var command = msg.Data;
            Log.Information(
                "[Device {DeviceId}] Apply irrigation → Field={FieldId}, Enable={EnableIrrigation}, Target={TargetMoisturePercent}%",
                DeviceId,
                command?.FieldId,
                command?.EnableIrrigation,
                command?.TargetMoisturePercent);
        }
    }
}

public sealed class IrrigationControllerWorker(
    INatsClient client,
    ILogger<IrrigationControllerWorker> logger) : BackgroundService
{
    private const string QueueGroupName = "irrigation-controllers";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var msg in client.SubscribeAsync<SoilMoistureReading>(
            subject: "sensors.soil.moisture.*",
            queueGroup: QueueGroupName,
            cancellationToken: stoppingToken))
        {
            var reading = msg.Data;
#pragma warning disable CA1873 // Avoid potentially expensive logging
            logger.LogInformation(
                "[Controller] Field={FieldId}, Moisture={MoisturePercent:F1}%",
                reading?.FieldId,
                reading?.MoisturePercent);
#pragma warning restore CA1873 // Avoid potentially expensive logging

            if (reading?.MoisturePercent < 20)
            {
                var command = new IrrigationCommand(
                    FieldId: reading.FieldId,
                    EnableIrrigation: true,
                    TargetMoisturePercent: 25,
                    TimestampUtc: DateTime.UtcNow);

                await client.PublishAsync(
                    "irrigation.commands",
                    command,
                    cancellationToken: stoppingToken);

                Log.Information("[Controller] Issued irrigation command for field {FieldId}", reading.FieldId);
            }
        }
    }
}
