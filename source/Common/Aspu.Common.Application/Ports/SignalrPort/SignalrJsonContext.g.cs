using System.Text.Json.Serialization;

namespace Aspu.Common.Application.Ports.SignalrPort;

[JsonSerializable(typeof(Test1Notification))]
[JsonSerializable(typeof(Test2Notification))]
public partial class SignalrJsonContext : JsonSerializerContext;

