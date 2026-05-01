using System;
using System.Linq;

namespace Withat.Tests.Tests;

public class NestedWithPathTests
{
    [ExtendedWith]
    public record Person
    {
        public required Address Address { get; init; }
    }

    public record Address
    {
        public required string City { get; init; }
    }

    [Test]
    public void OneLevelNestedPath_Value_Works()
    {
        var original = new Person { Address = new Address { City = "A" } };
        var actual = original.WithAddress_City("B");
        Assert.That(actual.Address.City, Is.EqualTo("B"));
        Assert.That(original.Address.City, Is.EqualTo("A"));
    }

    [ExtendedWith]
    public record Outer
    {
        public required A A { get; init; }
    }

    public record A
    {
        public required B B { get; init; }
    }

    public record B
    {
        public required int Leaf { get; init; }
    }

    [Test]
    public void NestedPath_Value_Works()
    {
        var original = new Outer { A = new A { B = new B { Leaf = 1 } } };
        var actual = original.WithA_B_Leaf(2);
        Assert.That(actual.A.B.Leaf, Is.EqualTo(2));
        Assert.That(original.A.B.Leaf, Is.EqualTo(1));
    }

    [Test]
    public void NestedPath_Func_Works()
    {
        var original = new Outer { A = new A { B = new B { Leaf = 1 } } };
        var actual = original.WithA_B_Leaf(x => x + 1);
        Assert.That(actual.A.B.Leaf, Is.EqualTo(2));
    }

    [Test]
    public void NestedPath_Func_ReceivesCurrentLeafValue()
    {
        // BuildReadFromLoadedChain uses targetIsLeaf to pick the right expression for
        // the func argument.  When targetIsLeaf=true the loop never declares v{last}
        // (it breaks on non-record segments), so the generated code must read
        // v{last-1}.Leaf — NOT v{last} (undeclared → compile error).
        // Removing targetIsLeaf from that method would produce v{last} and the project
        // would fail to build entirely, preventing this test from running at all.
        var original = new Outer { A = new A { B = new B { Leaf = 41 } } };
        int receivedByFunc = -1;

        original.WithA_B_Leaf(x =>
        {
            receivedByFunc = x;
            return x;
        });

        Assert.That(receivedByFunc, Is.EqualTo(41));
    }

    [Test]
    public void PrefixMethods_AreGenerated()
    {
        Assert.That(typeof(Outer_WithExtensions).GetMethods().Any(m => m.Name == "WithA"), Is.True);
        Assert.That(typeof(Outer_WithExtensions).GetMethods().Any(m => m.Name == "WithA_B"), Is.True);
        Assert.That(typeof(Outer_WithExtensions).GetMethods().Any(m => m.Name == "WithA_B_Leaf"), Is.True);
    }

    [ExtendedWith]
    public record OuterNullable
    {
        public A? A { get; init; }
    }

    [Test]
    public void TryWith_NullPropagation_ReturnsOriginalOnNullPath()
    {
        var original = new OuterNullable { A = null };
        var actual = original.TryWithA_B_Leaf(123);
        Assert.That(actual, Is.SameAs(original));
    }

    [Test]
    public void OrThrow_ThrowsOnNullPath()
    {
        var original = new OuterNullable { A = null };
        var ex = Assert.Throws<NullReferenceException>(() => original.WithA_B_Leaf_OrThrow(123));
        Assert.That(ex!.Message, Does.Contain("Withat"));
        Assert.That(ex.Message, Does.Contain("'A'"));
    }

    [ExtendedWith]
    public record OuterNoNested
    {
        [NoNestedWith]
        public required A A { get; init; }
    }

    [Test]
    public void NoNestedWith_BlocksNestedPathsButKeepsTopLevelWith()
    {
        // top-level WithA MUST exist
        Assert.That(typeof(OuterNoNested_WithExtensions).GetMethods().Any(m => m.Name == "WithA"), Is.True);

        // nested methods MUST NOT exist
        Assert.That(typeof(OuterNoNested_WithExtensions).GetMethods().Any(m => m.Name.StartsWith("WithA_")), Is.False);
        Assert.That(typeof(OuterNoNested_WithExtensions).GetMethods().Any(m => m.Name.StartsWith("TryWithA_")), Is.False);
    }

    [ExtendedWith]
    public record OuterNoNestedLeaf
    {
        public required A2 A { get; init; }
    }

    public record A2
    {
        public required B2 B { get; init; }
    }

    public record B2
    {
        [NoNestedWith]
        public required int Leaf { get; init; }
    }

    [Test]
    public void NoNestedWith_OnLeaf_IsNoOp()
    {
        // NoNestedWith on a non-record (leaf) is semantically meaningless and MUST NOT affect generation.
        Assert.That(typeof(OuterNoNestedLeaf_WithExtensions).GetMethods().Any(m => m.Name == "WithA_B_Leaf"), Is.True);
    }

    [ExtendedWith]
    public record OuterNoNestedDeep
    {
        public required A3 A { get; init; }
    }

    public record A3
    {
        [NoNestedWith]
        public required B3 B { get; init; }
    }

    public record B3
    {
        public required int Leaf { get; init; }
    }

    [Test]
    public void NoNestedWith_OnNonRootRecord_BlocksChildrenButKeepsNodeMethod()
    {
        // Method for the node itself MUST exist.
        Assert.That(typeof(OuterNoNestedDeep_WithExtensions).GetMethods().Any(m => m.Name == "WithA_B"), Is.True);

        // Children under that node MUST NOT be generated.
        Assert.That(typeof(OuterNoNestedDeep_WithExtensions).GetMethods().Any(m => m.Name.StartsWith("WithA_B_")), Is.False);
        Assert.That(typeof(OuterNoNestedDeep_WithExtensions).GetMethods().Any(m => m.Name.StartsWith("TryWithA_B_")), Is.False);
    }

    [ExtendedWith]
    public record OuterMixedAccessibility
    {
        public required A4 A { get; init; }
    }

    public record A4
    {
        public B4 B { get; internal init; }
    }

    public record B4
    {
        public required int Leaf { get; init; }
    }

    [Test]
    public void MixedAccessibility_PathBecomesInternal()
    {
        // Root A is public, but nested B is internal -> generated WithA_B_Leaf must be internal.
        var any = typeof(OuterMixedAccessibility_WithExtensions)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .FirstOrDefault(m => m.Name == "WithA_B_Leaf");
        Assert.That(any, Is.Not.Null);
        Assert.That(any!.IsPublic, Is.False);
        Assert.That(any.IsAssembly, Is.True);
    }

    [ExtendedWith]
    public record OuterCycle
    {
        public required CycleA A { get; init; }
    }

    public record CycleA
    {
        public required CycleB B { get; init; }
    }

    public record CycleB
    {
        public CycleA? Parent { get; init; }
        public required int Leaf { get; init; }
    }

    [Test]
    public void Cycles_DoNotCauseInfiniteGeneration()
    {
        var original = new OuterCycle { A = new CycleA { B = new CycleB { Parent = null, Leaf = 1 } } };
        var actual = original.WithA_B_Leaf(2);
        Assert.That(actual.A.B.Leaf, Is.EqualTo(2));
    }

}

