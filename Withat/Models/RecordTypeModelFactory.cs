using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Withat;

internal static class RecordTypeModelFactory
{
    internal static RecordTypeModel? Generate(GeneratorAttributeSyntaxContext context)
    {
        var typeSymbol = context.TargetSymbol as ITypeSymbol;
        if (typeSymbol == null)
            return null;
        var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
        
        var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var properties = new List<RecordPropertyModel>();
        foreach (var propSymbol in typeSymbol.GetMembers().OfType<IPropertySymbol>().Where(x=>!x.Name.Equals("EqualityContract")))
        {
            properties.Add(new RecordPropertyModel
            {
                PropertyName = propSymbol.Name,
                PropertyTypeFQ = propSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                SetAccessibility = propSymbol.SetMethod.DeclaredAccessibility,
                Attributes = propSymbol.GetAttributes().Select(x=>new AttributeModel
                {
                    AttributeName = x.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    Values = x.NamedArguments.ToImmutableDictionary(y => y.Key, y => y.Value.Value),
                }).ToImmutableArray()
            });
        }
        
        return new RecordTypeModel
        {
            RecordTypeNameMinified = typeName,
            RecordTypeNameFull = fullTypeName,
            RecordNamespaceName = namespaceName,
            Properties = properties.ToImmutableArray(),
        };
    }
}