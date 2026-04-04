using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace Aspu.Api.Extensions;

internal static class ObjectPoolExtensions
{
    internal static IServiceCollection AddObjectPool(
        this IServiceCollection services)
    {
        /// services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        /// services.AddSingleton(static sp =>
        /// {
        ///     var provider = sp.GetRequiredService<ObjectPoolProvider>();
        ///     return provider.CreateStringBuilderPool();
        /// });

        services.AddSingleton<ObjectPool<StringBuilder>>(static _ =>
            new DefaultObjectPool<StringBuilder>(new StringBuilderPolicy()));
        return services;
    }
}

internal sealed class StringBuilderPolicy(int MinCapacity = 256, int MaxCapacity = 4096)
    : PooledObjectPolicy<StringBuilder>
{
    public override StringBuilder Create() => new(MinCapacity);

    public override bool Return(StringBuilder sb)
    {
        sb.Clear();
        return sb.Capacity < MaxCapacity;
    }
}
