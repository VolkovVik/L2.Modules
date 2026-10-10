using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SourceGenerators.Presentation;

public abstract class BaseHttpRegistrationGenerator : BaseRegistrationGenerator
{
    protected BaseHttpRegistrationGenerator() { }

    protected static ImmutableHashSet<string> GetSymbolNames(ImmutableArray<INamedTypeSymbol?> classSymbols) =>
        classSymbols
            .Where(x => x is not null)
            .Select(x => x!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToImmutableHashSet(StringComparer.Ordinal);

    protected static ImmutableHashSet<string> GetSymbolNamespaces(ImmutableArray<INamedTypeSymbol?> classSymbols) =>
        classSymbols
            .Where(x => x is not null)
            .Select(x => x!.ContainingNamespace)
            .Where(x => !x.IsGlobalNamespace)
            .Select(x => x.ToDisplayString())
            .ToImmutableHashSet(StringComparer.Ordinal);
}

