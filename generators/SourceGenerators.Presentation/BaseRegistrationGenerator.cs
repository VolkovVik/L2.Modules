using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerators.Presentation;

public abstract class BaseRegistrationGenerator
{
    protected const string Using = "using ";
    private const string TopicPropertyName = "Topic";
    protected const string InboundProcessorNamespace = "Aspu.Common.Presentation.Abstractions.InboundProcessor";

    protected static readonly DiagnosticDescriptor NonConstantTopic = new(
        id: "ASPU001",
        title: "Inbound handler topic must be a constant",
        messageFormat: "Handler '{0}' implements {1} but its Topic isn't a compile-time constant string",
        category: "Aspu.Presentation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

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

    protected static IEnumerable<(string Name, string? Topic, INamedTypeSymbol Symbol)> GetInboundHandlers(SourceProductionContext context, Compilation compilation, ImmutableArray<INamedTypeSymbol?> classSymbols) =>
        classSymbols
            .Where(x => x is not null)
            .Distinct<INamedTypeSymbol?>(SymbolEqualityComparer.Default)
            .Select(x => (Name: x!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), Topic: GetConstantTopic(compilation, x, context.CancellationToken), Symbol: x))
            .OrderBy(x => x.Name, StringComparer.Ordinal);

    // Supports `Topic => "x"`, `Topic { get => "x"; }`, `Topic { get { return "x"; } }` and `Topic { get; } = "x"`,
    // including const fields and string concatenation of constants; the nearest declaration in the class hierarchy wins.
    private static string? GetConstantTopic(Compilation compilation, INamedTypeSymbol symbol, CancellationToken cancellationToken)
    {
        for (var type = symbol; type is not null; type = type.BaseType)
        {
            var property = type.GetMembers(TopicPropertyName).OfType<IPropertySymbol>().FirstOrDefault();
            if (property is null)
                continue;

            var syntax = property.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax(cancellationToken))
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault();
            if (syntax is null)
                return null;

            var expression = GetTopicExpression(syntax);
            if (expression is null)
                return null;

            var value = compilation.GetSemanticModel(expression.SyntaxTree).GetConstantValue(expression, cancellationToken);
            return value.HasValue ? value.Value as string : null;
        }

        return null;
    }

    private static ExpressionSyntax? GetTopicExpression(PropertyDeclarationSyntax syntax)
    {
        if (syntax.ExpressionBody is not null)
            return syntax.ExpressionBody.Expression;

        if (syntax.Initializer is not null)
            return syntax.Initializer.Value;

        var getter = syntax.AccessorList?.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
        if (getter?.ExpressionBody is not null)
            return getter.ExpressionBody.Expression;

        var statements = getter?.Body?.Statements;
        return statements is { Count: 1 } && statements.Value[0] is ReturnStatementSyntax { Expression: { } returned }
            ? returned
            : null;
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
