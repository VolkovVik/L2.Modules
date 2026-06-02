using System.Text.Json.Serialization;

namespace Aspu.Api.Adapters.SignalR;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Test1SignalrMessage), "test.1")]
[JsonDerivedType(typeof(Test2SignalrMessage), "test.2")]
public abstract record SignalrMessage(DateTime Timestamp);

public sealed record Test1SignalrMessage(string Description, DateTime Timestamp) : SignalrMessage(Timestamp);
public sealed record Test2SignalrMessage(string ErrorId, int Value, DateTime Timestamp) : SignalrMessage(Timestamp);
