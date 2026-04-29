using System;

namespace Withat.Tests.Tests;

/// <summary>
/// Tests for the case where a nullable record is the TARGET of assignment (not a traversal node).
/// The null-propagation guard must NOT fire when we are replacing the target — only when we
/// need to traverse THROUGH a null intermediate to reach the target.
/// </summary>
public class NullableTargetAssignmentTests
{
    // ── Depth-1 target (single-segment path, target IS the nullable record) ────────────

    [ExtendedWith]
    public record NtRoot1
    {
        public NtChild1? Child { get; init; }
    }

    public record NtChild1
    {
        public int Value { get; init; }
    }

    [Test]
    public void TryWith_Depth1_CanSetNullableRecordTarget_WhenTargetIsNull()
    {
        // Child is the TARGET (depth-1 path).  Its current value is null, but we are
        // replacing it — the TryWith guard must NOT short-circuit the assignment.
        var root = new NtRoot1 { Child = null };
        var newChild = new NtChild1 { Value = 42 };

        var result = root.TryWithChild(newChild);

        Assert.That(result.Child, Is.Not.Null);
        Assert.That(result.Child!.Value, Is.EqualTo(42));
    }

    [Test]
    public void TryWith_Depth1_CanSetNullableRecordTarget_WhenTargetIsNonNull()
    {
        // Sanity-check: replacing a non-null value must also work.
        var root = new NtRoot1 { Child = new NtChild1 { Value = 1 } };
        var newChild = new NtChild1 { Value = 99 };

        var result = root.TryWithChild(newChild);

        Assert.That(result.Child!.Value, Is.EqualTo(99));
    }

    [Test]
    public void OrThrow_Depth1_CanSetNullableRecordTarget_WhenTargetIsNull()
    {
        // OrThrow must NOT throw when the target itself is null — only traversal
        // nodes justify a throw.
        var root = new NtRoot1 { Child = null };
        var newChild = new NtChild1 { Value = 7 };

        NtRoot1 result = default!;
        Assert.DoesNotThrow(() => result = root.WithChild_OrThrow(newChild));
        Assert.That(result.Child!.Value, Is.EqualTo(7));
    }

    // ── Depth-2 target: non-nullable intermediate, nullable target ────────────────────

    [ExtendedWith]
    public record NtRoot2
    {
        public required NtInner2 Inner { get; init; }
    }

    public record NtInner2
    {
        public NtChild2? Target { get; init; }
    }

    public record NtChild2
    {
        public int Value { get; init; }
    }

    [Test]
    public void TryWith_Depth2_CanSetNullableRecordTarget_WhenTargetIsNull()
    {
        // Inner is non-null (safe to traverse).  Target (the destination) is null.
        // TryWith must proceed and set the new value.
        var root = new NtRoot2 { Inner = new NtInner2 { Target = null } };
        var newTarget = new NtChild2 { Value = 55 };

        var result = root.TryWithInner_Target(newTarget);

        Assert.That(result.Inner.Target, Is.Not.Null);
        Assert.That(result.Inner.Target!.Value, Is.EqualTo(55));
    }

    [Test]
    public void OrThrow_Depth2_CanSetNullableRecordTarget_WhenTargetIsNull()
    {
        var root = new NtRoot2 { Inner = new NtInner2 { Target = null } };
        var newTarget = new NtChild2 { Value = 33 };

        NtRoot2 result = default!;
        Assert.DoesNotThrow(() => result = root.WithInner_Target_OrThrow(newTarget));
        Assert.That(result.Inner.Target!.Value, Is.EqualTo(33));
    }

    // ── Regression: traversal null propagation must still work ────────────────────────

    [ExtendedWith]
    public record NtRoot3
    {
        public NtInner3? Inner { get; init; }
    }

    public record NtInner3
    {
        public required NtChild3 Child { get; init; }
    }

    public record NtChild3
    {
        public int Leaf { get; init; }
    }

    [Test]
    public void TryWith_TraversalNull_StillReturnsOriginal()
    {
        // Inner is null — this is a TRAVERSAL null, not a target null.
        // TryWith_Inner_Child_Leaf must still return the original record unchanged.
        var root = new NtRoot3 { Inner = null };

        var result = root.TryWithInner_Child_Leaf(99);

        Assert.That(result, Is.SameAs(root));
    }

    [Test]
    public void TryWith_TraversalNull_TryWithInner_ShouldStillReturnOriginal()
    {
        // TryWith_Inner where Inner (the target) is null — after the fix this SHOULD
        // succeed and set a new Inner.  Verify that the fix doesn't break the
        // TRAVERSAL-null case on a different method (Inner_Child_Leaf above).
        var root = new NtRoot3 { Inner = null };
        var newInner = new NtInner3 { Child = new NtChild3 { Leaf = 5 } };

        var result = root.TryWithInner(newInner);

        // Inner was null, but TryWithInner is a depth-1 assignment — must succeed.
        Assert.That(result.Inner, Is.Not.Null);
        Assert.That(result.Inner!.Child.Leaf, Is.EqualTo(5));
    }

    // ── Depth-3 target with null at depth-2 traversal node ───────────────────────────
    //
    // Structure: Root → A (non-null) → B? (nullable traversal node) → Leaf (int)
    //
    // B is null — it is NOT the target (Leaf is), so it IS a traversal node.
    // TryWith must return the original record unchanged.
    // OrThrow must throw with a message that identifies B and the method name.

    [ExtendedWith]
    public record NtRoot4
    {
        public required NtLevel4A A { get; init; }
    }

    public record NtLevel4A
    {
        public NtLevel4B? B { get; init; }
    }

    public record NtLevel4B
    {
        public int Leaf { get; init; }
    }

    [Test]
    public void TryWith_Depth3_TraversalNullAtLevel2_ReturnsOriginal()
    {
        // A is non-null, B (traversal) is null — can't reach Leaf.
        var root = new NtRoot4 { A = new NtLevel4A { B = null } };

        var result = root.TryWithA_B_Leaf(99);

        Assert.That(result, Is.SameAs(root));
    }

    [Test]
    public void TryWith_Depth3_TraversalNullAtLevel2_FuncOverload_ReturnsOriginal()
    {
        var root = new NtRoot4 { A = new NtLevel4A { B = null } };

        var result = root.TryWithA_B_Leaf(x => x + 1);

        Assert.That(result, Is.SameAs(root));
    }

    [Test]
    public void OrThrow_Depth3_TraversalNullAtLevel2_ThrowsWithInformativeMessage()
    {
        var root = new NtRoot4 { A = new NtLevel4A { B = null } };

        var ex = Assert.Throws<NullReferenceException>(() => root.WithA_B_Leaf_OrThrow(99));

        Assert.That(ex!.Message, Does.Contain("Withat"));
        // The message must identify the segment and the method that failed.
        Assert.That(ex.Message, Does.Contain("'B'"));
        Assert.That(ex.Message, Does.Contain("WithA_B_Leaf_OrThrow").Or.Contain("A.B"));
    }

    [Test]
    public void OrThrow_Depth3_TraversalNullAtLevel2_FuncOverload_ThrowsWithInformativeMessage()
    {
        var root = new NtRoot4 { A = new NtLevel4A { B = null } };

        var ex = Assert.Throws<NullReferenceException>(() => root.WithA_B_Leaf_OrThrow(x => x + 1));

        Assert.That(ex!.Message, Does.Contain("Withat"));
        Assert.That(ex.Message, Does.Contain("'B'"));
    }

    [Test]
    public void TryWith_Depth3_TraversalNonNull_UpdatesCorrectly()
    {
        // Sanity-check: when B is non-null the update must propagate all the way.
        var root = new NtRoot4 { A = new NtLevel4A { B = new NtLevel4B { Leaf = 1 } } };

        var result = root.TryWithA_B_Leaf(42);

        Assert.That(result.A.B!.Leaf, Is.EqualTo(42));
        Assert.That(root.A.B!.Leaf, Is.EqualTo(1), "original must not be mutated");
    }

    // ── Func overload: nullable record TARGET that is null ───────────────────────────
    //
    // Null propagation applies ONLY to traversal nodes — segments we pass THROUGH to
    // reach the target.  The target itself is never null-guarded: for value overloads
    // its current value is simply replaced; for func overloads the current value
    // (possibly null) is passed to the function as its argument.

    [ExtendedWith]
    public record NtFuncRoot
    {
        public NtFuncTarget? Target { get; init; }
    }

    public record NtFuncTarget
    {
        public int Value { get; init; }
    }

    [Test]
    public void TryWith_FuncOverload_NullableRecordTarget_WhenNull_CallsFuncWithNullAndUpdates()
    {
        // Target is null.  TryWith + func MUST call the func — null propagation does
        // not apply to the assignment target, only to traversal nodes.
        // The func receives null as its argument and returns a new value.
        var root = new NtFuncRoot { Target = null };
        NtFuncTarget? receivedArg = new NtFuncTarget { Value = -1 }; // sentinel

        var result = root.TryWithTarget(t =>
        {
            receivedArg = t;
            return new NtFuncTarget { Value = 99 };
        });

        Assert.That(receivedArg, Is.Null, "func must receive the current (null) target value");
        Assert.That(result.Target, Is.Not.Null);
        Assert.That(result.Target!.Value, Is.EqualTo(99));
    }

    [Test]
    public void TryWith_FuncOverload_NullableRecordTarget_WhenNonNull_CallsFuncAndUpdates()
    {
        // Sanity-check: when Target is non-null the func must receive the current value
        // and the result must be applied.
        var root = new NtFuncRoot { Target = new NtFuncTarget { Value = 1 } };

        var result = root.TryWithTarget(t => t! with { Value = 42 });

        Assert.That(result.Target!.Value, Is.EqualTo(42));
    }

    [Test]
    public void OrThrow_FuncOverload_NullableRecordTarget_WhenNull_CallsFuncWithNullAndUpdates()
    {
        // OrThrow must also NOT throw when the target itself is null — it should still
        // call the func.  Only traversal nodes justify a throw.
        var root = new NtFuncRoot { Target = null };

        NtFuncRoot result = default!;
        Assert.DoesNotThrow(() =>
            result = root.WithTarget_OrThrow(t =>
            {
                Assert.That(t, Is.Null);
                return new NtFuncTarget { Value = 7 };
            }));
        Assert.That(result.Target!.Value, Is.EqualTo(7));
    }

    // ── Traversal null STILL propagates for func overloads ───────────────────────

    [ExtendedWith]
    public record NtFuncRootDeep
    {
        public NtFuncInner? Inner { get; init; }
    }

    public record NtFuncInner
    {
        public NtFuncTarget? Target { get; init; }
    }

    [Test]
    public void TryWith_FuncOverload_TraversalNullAtInner_ReturnsOriginalWithoutCallingFunc()
    {
        // Inner is null — this IS a traversal node, so null propagation fires and
        // the func must NOT be called.
        var root = new NtFuncRootDeep { Inner = null };
        var funcWasCalled = false;

        var result = root.TryWithInner_Target(_ =>
        {
            funcWasCalled = true;
            return new NtFuncTarget { Value = 99 };
        });

        Assert.That(funcWasCalled, Is.False, "func must not be called when a traversal node is null");
        Assert.That(result, Is.SameAs(root));
    }
}
