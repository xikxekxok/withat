using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Withat.Sample;

[ExtendedWith]

public record SampleAggregate
{
    public int P1 { get; private set; }
    public int P2 { get; private init; }
    public int Primitive { get; init; }
    public SampleChild Complex { get; init; }
    public ImmutableDictionary<string, SampleChild> Dict { get; init; }
    [ExtendedWithIgnore]
    public ImmutableList<SampleChild> List { get; init; }
    public ImmutableArray<int> Array { get; init; }
}

[ExtendedWith]
public record SampleChild(int Quantity, SampleAttributes Attributes);

[ExtendedWith]
public record SampleAttributes(string Attr1, string Attr2);
