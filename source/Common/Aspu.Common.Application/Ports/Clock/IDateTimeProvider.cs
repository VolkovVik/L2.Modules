namespace Aspu.Common.Application.Ports.Clock;

public interface IDateTimeProvider
{
#pragma warning disable IDE0040 // Remove accessibility modifiers
    public DateTime UtcNow { get; }
#pragma warning restore IDE0040 // Remove accessibility modifiers
}
