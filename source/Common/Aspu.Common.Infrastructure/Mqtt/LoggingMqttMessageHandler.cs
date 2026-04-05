using Aspu.Common.Application.Ports.Messaging;
using Microsoft.Extensions.Logging;

namespace Aspu.Common.Infrastructure.Mqtt;

internal sealed class LoggingMqttMessageHandler(ILogger<LoggingMqttMessageHandler> logger) : IMqttMessageHandler
{
    public Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "MQTT message on {Topic}, length {Length} bytes",
                topic,
                payload.Length);
        }

        return Task.CompletedTask;
    }
}
