namespace Aspu.Common.Infrastructure.Mqtt;

/// <summary>
/// MQTT connection quality: keep-alive, timeouts, and subscription QoS.
/// </summary>
public sealed class MqttConnectionQualityOptions
{
    /// <summary>
    /// MQTT keep-alive interval (PINGREQ/PINGRESP). 0 leaves the client library default.
    /// </summary>
    public int KeepAliveSeconds { get; init; }

    /// <summary>
    /// Timeout for socket and internal client operations. 0 leaves the library default.
    /// </summary>
    public int CommunicationTimeoutSeconds { get; init; }

    /// <summary>
    /// QoS level applied to every subscribe topic filter: 0 = at most once, 1 = at least once, 2 = exactly once.
    /// </summary>
    public int SubscribeQualityOfService { get; init; }
}
