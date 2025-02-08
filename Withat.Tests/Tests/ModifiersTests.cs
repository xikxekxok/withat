using System.Reflection;

namespace Withat.Tests.Tests;

public class ModifiersTests
{
    [ExtendedWith]
    public record ModifiersTestObject(int Prop1)
    {
        public required int Prop2 { get; init; }
        public required int Prop3 { get; set; }
        public int Prop4 { get; set; }
        public int Prop5 { get; init; }
        internal int Prop6 { get; init; }
        public int Prop7 { get; internal set; }
        //no
        protected int Prop8 { get; init; }
        //no
        private int Prop9 { get; init; }
        //no
        public int Prop10 { get; private set; }
        //no
        public int Prop11 { get; protected set; }

    }

    [Test]
    public void ModifiersExecutionTests()
    {
        var original = new ModifiersTestObject(-1)
        {
            Prop2 = -2,
            Prop3 = -3,
            Prop4 = -4,
            Prop5 = -5,
            Prop6 = -6,
            Prop7 = -6,
        };

        var actual = original
            .WithProp1(1)
            .WithProp2(2)
            .WithProp3(3)
            .WithProp4(4)
            .WithProp5(5)
            .WithProp6(6)
            .WithProp7(7);
        
        Assert.That(actual.Prop1, Is.EqualTo(1));
        Assert.That(actual.Prop2, Is.EqualTo(2));
        Assert.That(actual.Prop3, Is.EqualTo(3));
        Assert.That(actual.Prop4, Is.EqualTo(4));
        Assert.That(actual.Prop5, Is.EqualTo(5));
        Assert.That(actual.Prop6, Is.EqualTo(6));
        Assert.That(actual.Prop7, Is.EqualTo(7));
    }

    [Test]
    public void Modifiers_CheckMethods()
    {
        var allPublicMethods = typeof(ModifiersTestObject_WithExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(x=>x.Name)
            .ToHashSet();
        Assert.That(allPublicMethods, Has.Count.EqualTo(5));
        Assert.That(allPublicMethods, Has.Member("WithProp1"));
        Assert.That(allPublicMethods, Has.Member("WithProp2"));
        Assert.That(allPublicMethods, Has.Member("WithProp3"));
        Assert.That(allPublicMethods, Has.Member("WithProp4"));
        Assert.That(allPublicMethods, Has.Member("WithProp5"));
        
        var allInternalMethods = typeof(ModifiersTestObject_WithExtensions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Select(x=>x.Name)
            .ToHashSet();
        Assert.That(allInternalMethods, Has.Count.EqualTo(2));
        Assert.That(allInternalMethods, Has.Member("WithProp6"));
        Assert.That(allInternalMethods, Has.Member("WithProp7"));
    }
}