using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerators.Presentation;

public abstract class BaseHttpRegistrationGenerator : BaseRegistrationGenerator
{
    protected BaseHttpRegistrationGenerator() { }

    protected static bool IsCandidate(SyntaxNode node) =>
        node is ClassDeclarationSyntax { BaseList: not null };

    protected static INamedTypeSymbol? GetSemanticTarget(GeneratorSyntaxContext context, string interfaceName, CancellationToken cancellationToken)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classDecl, cancellationToken) is not INamedTypeSymbol symbol)
            return null;

        if (!symbol.IsSealed)
            return null;

        if (symbol.TypeKind is not TypeKind.Class)
            return null;

        if (symbol.DeclaredAccessibility is not (Accessibility.Internal or Accessibility.Public))
            return null;

        return symbol.AllInterfaces.Any(a => string.Equals(a.Name, interfaceName, StringComparison.Ordinal)) ? symbol : null;
    }

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

