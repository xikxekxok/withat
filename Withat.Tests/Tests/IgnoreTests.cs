namespace Withat.Tests.Tests;

public class IgnoreTests
{
    [ExtendedWith]
    public record IgnoreTestObject
    {
        [ExtendedWithIgnore]
        public int Prop1 { get; init; }
        public int Prop2 { get; init; }
    }

    [Test]
    public void WithIgnoreAttribute_NotGenerated()
    {
        var allMethods = typeof(IgnoreTestObject_WithExtensions).GetMethods()
            .Select(x=>x.Name)
            .ToHashSet();
        Assert.That(allMethods, Has.Member("WithProp2"));
        Assert.That(allMethods, Has.No.Member("WithProp1"));
    }
}