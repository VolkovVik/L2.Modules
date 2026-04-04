using System.Text;
using Aspu.Common.Application.Extensions;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.ObjectPool;

[MemoryDiagnoser]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0047:Declare types in namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S3903:Types should be defined in named namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1110:Declare type inside namespace", Justification = "<Pending>")]
public class StringBuilderTests
{
    private readonly ObjectPool<StringBuilder> _pool;
    private readonly DefaultObjectPool<StringBuilder> _pool1;

    public StringBuilderTests()
    {
        var provider = new DefaultObjectPoolProvider();
        _pool = provider.CreateStringBuilderPool();

        _pool1 = new DefaultObjectPool<StringBuilder>(new GeneratorStringBuilderPolicy());
    }

    [Benchmark(Baseline = true)]
    public string WithoutPool()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < 100; i++)
        {
            sb.Append("Hello ");
            sb.Append(i);
            sb.Append(' ');
        }
        return sb.ToString();
    }

    [Benchmark]
    public string StringBuilder_Pool()
    {
        var sb = _pool.Get();
        try
        {
            for (var i = 0; i < 100; i++)
            {
                sb.Append("Hello ");
                sb.Append(i);
                sb.Append(' ');
            }

            return sb.ToString();
        }
        finally
        {
            _pool.Return(sb);
        }
    }

    [Benchmark]
    public string StringBuilder_Pool11()
    {
        return _pool1.Build(sb =>
        {
            for (var i = 0; i < 100; i++)
            {
                sb.Append("Hello ");
                sb.Append(i);
                sb.Append(' ');
            }
        });
    }

    public sealed class GeneratorStringBuilderPolicy(int InitCapacity = 512, int MaxCapacity = 8192) : PooledObjectPolicy<StringBuilder>
    {
        public override StringBuilder Create() =>
            new(capacity: InitCapacity);

        public override bool Return(StringBuilder sb)
        {
            sb.Clear();
            return sb.Capacity <= MaxCapacity;
        }
    }
}
