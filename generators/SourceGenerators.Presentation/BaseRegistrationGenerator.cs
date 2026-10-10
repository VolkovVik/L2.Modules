using Microsoft.CodeAnalysis;

namespace SourceGenerators.Presentation;

public abstract class BaseRegistrationGenerator
{
    protected const string Using = "using ";

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

