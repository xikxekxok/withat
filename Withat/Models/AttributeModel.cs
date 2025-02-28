using System.Collections.Immutable;

namespace Withat.Models;

public record AttributeModel
{
    public required string AttributeName { get; init; }
    public required ImmutableDictionary<string, object?> Values { get; init; }
}