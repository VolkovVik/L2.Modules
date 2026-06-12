using System.Text.Json.Serialization;

namespace Aspu.Common.Application.Ports.SignalrPort;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Test1Notification))]
[JsonSerializable(typeof(Test2Notification))]
[JsonSerializable(typeof(Test3Notification))]
public partial class SignalrJsonContext : JsonSerializerContext;
