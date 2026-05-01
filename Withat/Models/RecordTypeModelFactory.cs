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

        return new RecordTypeModel
        {
            RecordTypeNameMinified = typeName,
            RecordTypeNameFull = fullTypeName,
            RecordNamespaceName = namespaceName,
            Properties = BuildRecordProperties(typeSymbol, []).ToImmutableArray(),
        };
    }

    private static IEnumerable<RecordPropertyModel> BuildRecordProperties(ITypeSymbol typeSymbol, HashSet<string> visitedRecordTypes)
    {
        var properties = typeSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(x => !x.Name.Equals("EqualityContract"));
        foreach (var propSymbol in properties)
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

            var nested = ImmutableArray<RecordPropertyModel>.Empty;
            if (isRecord && unwrappedType is INamedTypeSymbol recordType)
            {
                // Prevent cycles by type identity string
                var key = recordType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (!visitedRecordTypes.Contains(key))
                {
                    var nextVisited = new HashSet<string>(visitedRecordTypes) { key };
                    nested = BuildRecordProperties(recordType, nextVisited).ToImmutableArray();
                }
            }

            yield return new RecordPropertyModel
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