using Aspu.Common.Application.Ports.Clock;

namespace Aspu.Common.Infrastructure.Adapters.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
