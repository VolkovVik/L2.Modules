using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerators.Presentation;

public abstract class BaseHandlersRegistrationGenerator : BaseRegistrationGenerator
{
    private const string TopicPropertyName = "Topic";
    protected const string InboundProcessorNamespace = "Aspu.Common.Presentation.Abstractions.InboundProcessor";

    protected static readonly DiagnosticDescriptor NonConstantTopic = new(
        id: "ASPU001",
        title: "Inbound handler topic must be a compile-time constant string",
        messageFormat: "Handler '{0}' implements {1} but its Topic isn't a compile-time constant string",
        category: "Aspu.Presentation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    protected static readonly DiagnosticDescriptor NotRegistrableHandler = new(
        id: "ASPU002",
        title: "Inbound handler can't be registered",
        messageFormat: "Handler '{0}' implements {1} but isn't registered: it must be sealed and internal or public (including containing types)",
        category: "Aspu.Presentation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    protected BaseHandlersRegistrationGenerator() { }

    /// <summary>
    /// Every concrete class implementing <paramref name="interfaceName"/>, unlike <see cref="GetSemanticTarget"/>:
    /// unsuitable handlers are reported (<see cref="NotRegistrableHandler"/>) instead of being silently skipped.
    /// Abstract classes are bases, not handlers, so they're ignored.
    /// </summary>
    protected static INamedTypeSymbol? GetInboundHandlerTarget(GeneratorSyntaxContext context, string interfaceName, CancellationToken cancellationToken)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classDecl, cancellationToken) is not INamedTypeSymbol symbol)
            return null;

        if (symbol.TypeKind is not TypeKind.Class || symbol.IsAbstract || symbol.IsStatic)
            return null;

        return symbol.AllInterfaces.Any(a => string.Equals(a.Name, interfaceName, StringComparison.Ordinal)) ? symbol : null;
    }

    /// <summary>Generated registration code must reference the handler, and handlers are sealed by convention.</summary>
    protected static bool IsRegistrableHandler(INamedTypeSymbol symbol)
    {
        if (!symbol.IsSealed)
            return false;

        for (var type = symbol; type is not null; type = type.ContainingType)
        {
            if (type.DeclaredAccessibility is not (Accessibility.Internal or Accessibility.Public))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Handlers ordered by name; <c>TopicLiteral</c> is the trimmed constant <c>Topic</c> as a ready-to-emit C# string literal
    /// (quoted and escaped), or null when <c>Topic</c> isn't a compile-time constant.
    /// </summary>
    protected static IEnumerable<(string Name, string? TopicLiteral, INamedTypeSymbol Symbol)> GetInboundHandlers(SourceProductionContext context, Compilation compilation, ImmutableArray<INamedTypeSymbol?> classSymbols) =>
        classSymbols
            .Where(x => x is not null)
            .Distinct<INamedTypeSymbol?>(SymbolEqualityComparer.Default)
            .Select(x => (
                Name: x!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                TopicLiteral: ToTopicLiteral(GetConstantTopic(compilation, x, context.CancellationToken)),
                Symbol: x))
            .OrderBy(x => x.Name, StringComparer.Ordinal);

    // Trimmed like the runtime registry used to do, then escaped so the emitted literal holds exactly the computed value.
    private static string? ToTopicLiteral(string? topic) =>
        topic is null ? null : SymbolDisplay.FormatLiteral(topic.Trim(), quote: true);

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
}

