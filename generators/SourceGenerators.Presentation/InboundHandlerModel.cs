using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerators.Presentation;

/// <summary>
/// Everything generation needs about one handler, computed in the pipeline transform.
/// Only values (no symbols or syntax): unchanged handlers compare equal, so the output step is cached,
/// and no symbol keeps an old compilation alive.
/// </summary>
internal sealed record InboundHandlerModel(
    string FullName,
    string Name,
    string? Topic,
    bool IsRegistrable,
    bool IsGeneric,
    LocationInfo? Location);

/// <summary>Value stand-in for <see cref="Microsoft.CodeAnalysis.Location"/>, which keeps the syntax tree alive.</summary>
internal sealed record LocationInfo(string FilePath, TextSpan Span, LinePositionSpan LineSpan)
{
    public Location ToLocation() => Location.Create(FilePath, Span, LineSpan);

    public static LocationInfo? From(ISymbol symbol)
    {
        var location = symbol.Locations.FirstOrDefault();
        return location?.SourceTree is null
            ? null
            : new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }
}
