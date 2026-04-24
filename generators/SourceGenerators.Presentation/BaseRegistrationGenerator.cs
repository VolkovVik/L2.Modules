using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerators.Presentation;

public abstract class BaseRegistrationGenerator
{
    protected const string Using = "using ";

    protected BaseRegistrationGenerator() { }

    protected static bool IsCandidate(SyntaxNode node) =>
        node is ClassDeclarationSyntax { BaseList: not null };

    protected static INamedTypeSymbol? GetSemanticTarget(GeneratorSyntaxContext context, string interfaceName, CancellationToken cancellationToken)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classDecl, cancellationToken) is not INamedTypeSymbol symbol)
            return null;

        if (symbol.TypeKind is not TypeKind.Class)
            return null;

        if (symbol.DeclaredAccessibility is not (Accessibility.Internal or Accessibility.Public))
            return null;

        if (!symbol.IsSealed)
            return null;

        return symbol.AllInterfaces.Any(a =>
            string.Equals(a.Name, interfaceName, StringComparison.Ordinal))
                ? symbol : null;
    }

#pragma warning disable MA0007 // Add a comma after the last value
    protected static ImmutableHashSet<string> GetSymbolNames(ImmutableArray<INamedTypeSymbol?> classSymbols) =>
        [.. classSymbols
            .Where(x => x is not null)
            .Select(x => x!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            .Where(x => !string.IsNullOrWhiteSpace(x))];

    protected static ImmutableHashSet<string> GetSymbolNamespaces(ImmutableArray<INamedTypeSymbol?> classSymbols) =>
        [.. classSymbols
            .Where(x => x is not null)
            .Select(x => x!.ContainingNamespace)
            .Where(x => !x.IsGlobalNamespace)
            .Select(x => x.ToDisplayString())];
#pragma warning restore MA0007 // Add a comma after the last value

    protected static string GetNamespace(Compilation compilation, string namespaceName, string interfaceName, string metadataName)
    {
        var interfaceSymbol = compilation.GetTypeByMetadataName(metadataName) ?? FindInterface(compilation, interfaceName);
        return interfaceSymbol?.ContainingNamespace.IsGlobalNamespace != false
            ? namespaceName
            : interfaceSymbol.ContainingNamespace.ToDisplayString();
    }

    private static INamedTypeSymbol? FindInterface(Compilation compilation, string name) =>
        FindInNamespace(compilation.GlobalNamespace, name);

    private static INamedTypeSymbol? FindInNamespace(INamespaceSymbol ns, string name)
    {
        foreach (var member in ns.GetMembers())
        {
            if (member is INamespaceSymbol childNs)
            {
                var found = FindInNamespace(childNs, name);
                if (found is not null)
                    return found;

                continue;
            }

            if (member is INamedTypeSymbol type && type.TypeKind == TypeKind.Interface && string.Equals(type.Name, name, StringComparison.OrdinalIgnoreCase))
                return type;
        }
        return null;
    }
}
