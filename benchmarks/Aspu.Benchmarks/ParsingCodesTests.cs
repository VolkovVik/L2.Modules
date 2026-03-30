using Aspu.Modules.Orders.Application;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0047:Declare types in namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S3903:Types should be defined in named namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1110:Declare type inside namespace", Justification = "<Pending>")]
public class ParsingCodesTests
{
    [Benchmark]
    public void Parse() =>
        CodesParsing.Parse("0104636332455360210000000033461\u001d10ddddd\u001d111234561712345693abcd");

    [Benchmark]
    public void Transform() =>
       CodesParsing.Transform("0104636332455360210000000033461\u001d10ddddd\u001d111234561712345693abcd");
}
