using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Withat.SourceCode;

namespace Withat;

[Generator]
public class WithGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(initializationContext => initializationContext
            .AddSource("ExtendedWithAttribute.cs", Attributes.SourceCode)
        );
        context.RegisterPostInitializationOutput(initializationContext => initializationContext
            .AddSource("ImmutableCollectionsUpdateExtensions.cs", ImmutableCollectionsWithExtensions.GenerateSourceCode())
        );
        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Withat.ExtendedWithAttribute", 
                (node, _) => node is RecordDeclarationSyntax,
            (c, token) => RecordTypeModelFactory.Generate(c)
            )
            .Where(x=>x!=null);
        
        context.RegisterSourceOutput(provider, (productionContext, recordTypeModel) =>
        {
            productionContext.AddSource($"{recordTypeModel.RecordTypeNameMinified}_WithExtensions.g.cs", RecordWithExtensions.Generate(recordTypeModel));
        });
    }
}