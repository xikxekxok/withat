using System.Collections.Immutable;

namespace Withat.Tests.Tests;

public class ImmutableDictionaryTests
{
    [Test]
    public void Dict_NoValue_Throws()
    {
        var original = ImmutableDictionary<string, int>.Empty;
        var ex = Assert.Catch<KeyNotFoundException>(() => original.With("notFound", i => i + 1));
        Assert.That(ex.Message, Does.Contain("No value for key 'notFound' in 'original'!"));
    }
    
    [Test]
    public void Dict_ValueExist_Updated()
    {
        var original = ImmutableDictionary<string, int>.Empty
            .Add("key1", 1)
            .Add("key2", 2)
            .Add("key3", 3);
        var actual = original.With("key2", i => i+5);
        
        Assert.That(actual.Count, Is.EqualTo(3));
        Assert.That(actual["key1"], Is.EqualTo(1));
        Assert.That(actual["key2"], Is.EqualTo(7));
        Assert.That(actual["key3"], Is.EqualTo(3));
    }
    
    [Test]
    public async Task Dict_ValueExist_UpdatedAsync()
    {
        var original = ImmutableDictionary<string, int>.Empty
            .Add("key1", 1)
            .Add("key2", 2)
            .Add("key3", 3);
        var actual = await original.With("key2", async i => i+5);
        
        Assert.That(actual.Count, Is.EqualTo(3));
        Assert.That(actual["key1"], Is.EqualTo(1));
        Assert.That(actual["key2"], Is.EqualTo(7));
        Assert.That(actual["key3"], Is.EqualTo(3));
    }
    
    [Test]
    public void DictInterface_NoValue_Throws()
    {
        IImmutableDictionary<string, int> original = ImmutableDictionary<string, int>.Empty;
        var ex = Assert.Catch<KeyNotFoundException>(() => original.With("notFound", i => i + 1));
        Assert.That(ex.Message, Does.Contain("No value for key 'notFound' in 'original'!"));
    }
    
    [Test]
    public void DictInterface_ValueExist_Updated()
    {
        IImmutableDictionary<string, int> original = ImmutableDictionary<string, int>.Empty
            .Add("key1", 1)
            .Add("key2", 2)
            .Add("key3", 3);
        var actual = original.With("key2", i => i+5);
        
        Assert.That(actual.Count, Is.EqualTo(3));
        Assert.That(actual["key1"], Is.EqualTo(1));
        Assert.That(actual["key2"], Is.EqualTo(7));
        Assert.That(actual["key3"], Is.EqualTo(3));
    }
    
    [Test]
    public async Task DictInterface_ValueExist_UpdatedAsync()
    {
        IImmutableDictionary<string, int> original = ImmutableDictionary<string, int>.Empty
            .Add("key1", 1)
            .Add("key2", 2)
            .Add("key3", 3);
        var actual = await original.With("key2", async i => i+5);
        
        Assert.That(actual.Count, Is.EqualTo(3));
        Assert.That(actual["key1"], Is.EqualTo(1));
        Assert.That(actual["key2"], Is.EqualTo(7));
        Assert.That(actual["key3"], Is.EqualTo(3));
    }
}