namespace Withat.Tests.Tests;

public class PropertyTests
{
    #region testData
    [ExtendedWith]
    public record TestAggregate
    {
        public int Primitive { get; init; }
        public TestChild Complex { get; init; }
    }

    [ExtendedWith]
    public record TestChild(string ChildValue);
    
    private TestAggregate _original = new TestAggregate
    {
        Primitive = 15,
        Complex = new TestChild("Hello")
    };
    private TestAggregate _expected = new TestAggregate
    {
        Primitive = 20,
        Complex = new TestChild("Hello World")
    };
    
    #endregion
    
    
    
    [Test]
    public void FromRecord_Value()
    {
        var actual = _original
            .WithPrimitive(20)
            .WithComplex(new TestChild("Hello World"));
        Assert.That(actual, Is.EqualTo(_expected));
    }
    
    [Test]
    public async Task FromRecord_TaskWithValue()
    {
        var actual = await _original.WithPrimitive(Task.FromResult(20));
        actual = await actual.WithComplex(Task.FromResult(new TestChild("Hello World")));
        Assert.That(actual, Is.EqualTo(_expected));
    }

    [Test]
    public void FromRecord_Func()
    {
        var actual = _original
            .WithPrimitive(p=>p+5)
            .WithComplex(c=>c.WithChildValue(v=>v+" World"));
        Assert.That(actual, Is.EqualTo(_expected));
    }
    
    [Test]
    public async Task FromRecord_AsyncFunc()
    {
        var actual = await _original.WithPrimitive(async p=>p+5);
        actual = await actual.WithComplex(async c=>await c.WithChildValue(async v=>v+" World"));
        Assert.That(actual, Is.EqualTo(_expected));
    }
    
    
    [Test]
    public async Task FromTask_Value()
    {
        var actual = await Task.FromResult(_original)
            .WithPrimitive(20)
            .WithComplex(new TestChild("Hello World"));
        Assert.That(actual, Is.EqualTo(_expected));
    }
    
    [Test]
    public async Task FromTask_TaskWithValue()
    {
        var actual = await Task.FromResult(_original)
            .WithPrimitive(Task.FromResult(20))
            .WithComplex(Task.FromResult(new TestChild("Hello World")));
        Assert.That(actual, Is.EqualTo(_expected));
    }

    [Test]
    public async Task FromTask_Func()
    {
        var actual = await Task.FromResult(_original)
            .WithPrimitive(p=>p+5)
            .WithComplex(c=>c
                .WithChildValue(v=>v+" World")
            );
        Assert.That(actual, Is.EqualTo(_expected));
    }
    
    [Test]
    public async Task FromTask_AsyncFunc()
    {
        var actual = await Task.FromResult(_original)
            .WithPrimitive(async p=>p+5)
            .WithComplex(async c=>await c
                .WithChildValue(async v=>v+" World")
            );
        Assert.That(actual, Is.EqualTo(_expected));
    }
}