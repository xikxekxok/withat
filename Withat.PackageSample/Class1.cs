namespace Withat.PackageSample;

[ExtendedWith]
public record Sample
{
    public int Prop1 { get; init; }
    [ExtendedWithIgnore]
    public int Prop2 { get; set; }
    public int Prop3 { get; }
}