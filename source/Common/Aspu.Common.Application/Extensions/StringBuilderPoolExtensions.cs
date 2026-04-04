using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace Aspu.Common.Application.Extensions;

public static class StringBuilderPoolExtensions
{
    extension(ObjectPool<StringBuilder> pool)
    {
        public string Build(Action<StringBuilder> action)
        {
            var sb = pool.Get();
            try
            {
                action(sb);
                return sb.ToString();
            }
            finally
            {
                pool.Return(sb);
            }
        }
    }
}
