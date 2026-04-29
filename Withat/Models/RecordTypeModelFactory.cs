using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Withat.Models;

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

        var properties = typeSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(x => !x.Name.Equals("EqualityContract"))
            .Select(p => BuildPropertyModel(p, new HashSet<string>()))
            .ToList();
        
        return new RecordTypeModel
        {
            RecordTypeNameMinified = typeName,
            RecordTypeNameFull = fullTypeName,
            RecordNamespaceName = namespaceName,
            Properties = properties.ToImmutableArray(),
        };
    }

    private static RecordPropertyModel BuildPropertyModel(IPropertySymbol propSymbol, HashSet<string> visitedRecordTypes)
    {
        var type = propSymbol.Type;
        var (unwrappedType, isNullable) = UnwrapNullable(type);
        var isRecord = unwrappedType is INamedTypeSymbol nts && nts.IsRecord;

        var hasIgnoreAttribute = propSymbol.GetAttributes()
            .Any(x =>
                x.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Equals("global::Withat.ExtendedWithIgnoreAttribute") == true);

        var hasNoNestedWithAttribute = propSymbol.GetAttributes()
            .Any(x =>
                x.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Equals("global::Withat.NoNestedWithAttribute") == true);

        ImmutableArray<RecordPropertyModel> nested = ImmutableArray<RecordPropertyModel>.Empty;
        if (isRecord && unwrappedType is INamedTypeSymbol recordType)
        {
            // Prevent cycles by type identity string
            var key = recordType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (!visitedRecordTypes.Contains(key))
            {
                var nextVisited = new HashSet<string>(visitedRecordTypes) { key };
                nested = recordType
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(x => !x.Name.Equals("EqualityContract"))
                    .Select(p => BuildPropertyModel(p, nextVisited))
                    .ToImmutableArray();
            }
        }

        return new RecordPropertyModel
        {
            PropertyName = propSymbol.Name,
            PropertyTypeFQ = propSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            SetAccessibility = propSymbol.SetMethod?.DeclaredAccessibility,
            HasIgnoreAttribute = hasIgnoreAttribute,
            HasNoNestedWithAttribute = hasNoNestedWithAttribute,
            IsRecord = isRecord,
            IsNullable = isNullable,
            NestedProperties = nested,
        };
    }

    private static (ITypeSymbol Unwrapped, bool IsNullable) UnwrapNullable(ITypeSymbol type)
    {
        // Nullable<T>
        if (type is INamedTypeSymbol named &&
            named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
            named.TypeArguments.Length == 1)
        {
            return (named.TypeArguments[0], true);
        }

        // Reference type nullability annotation
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return (type.WithNullableAnnotation(NullableAnnotation.NotAnnotated), true);
        }

        return (type, false);
    }
}