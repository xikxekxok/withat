using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Withat;

namespace Withat.Tests.Tests;

public class PathSuffixCollisionTests
{
    [Test]
    public void PathSuffix_A_B_Leaf_Collision_StopsGeneration()
    {
        var source = @"
using System;

namespace Withat
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ExtendedWithAttribute : Attribute {}
}

[Withat.ExtendedWith]
public record OuterCollision
{
    public required CollisionA A { get; init; }
    public required CollisionAB A_B { get; init; }
}

public record CollisionA
{
    public required CollisionB B { get; init; }
}

public record CollisionB
{
    public required int Leaf { get; init; }
}

public record CollisionAB
{
    public required int Leaf { get; init; }
}
";

        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
        var references = GetDefaultReferences();

        var compilation = CSharpCompilation.Create(
            assemblyName: "PathSuffixCollisionTest",
            syntaxTrees: new[] { tree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new WithGenerator();

        Exception? caught = null;
        ImmutableArray<Diagnostic> diagnostics;

        try
        {
            GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { (IIncrementalGenerator)generator });
            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var runDiagnostics);
            diagnostics = runDiagnostics;
        }
        catch (Exception ex)
        {
            caught = ex;
            diagnostics = default;
        }

        if (caught is not null)
        {
            Assert.That(caught, Is.TypeOf<InvalidOperationException>());
            Assert.That(caught.Message, Does.Contain("duplicate planned path"));
        Assert.That(caught.Message, Does.Contain("WithA_B"));
            return;
        }

        var diagText = string.Join("\n", diagnostics.Select(d => d.ToString()));
        Assert.That(diagText, Does.Contain("duplicate planned path"), diagText);
    Assert.That(diagText, Does.Contain("A_B"), diagText);
    }

    private static IEnumerable<MetadataReference> GetDefaultReferences()
    {
        var trustedAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (trustedAssemblies is null)
            throw new InvalidOperationException("TRUSTED_PLATFORM_ASSEMBLIES is not set.");

        return trustedAssemblies
            .Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p));
    }
}

