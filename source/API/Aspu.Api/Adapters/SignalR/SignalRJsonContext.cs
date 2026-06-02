using System.Text.Json.Serialization;
using Aspu.Common.Presentation.Abstractions.SignalR;

namespace Aspu.Api.Adapters.SignalR;

[JsonSerializable(typeof(Test1Notification))]
[JsonSerializable(typeof(Test2Notification))]
internal partial class SignalRJsonContext : JsonSerializerContext;
