using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Withat.Models;

public record RecordPropertyModel
{
    public required string PropertyName { get; init; }
    public required string PropertyTypeFQ { get; init; }
    public required Accessibility? SetAccessibility { get; init; }
    public required bool HasIgnoreAttribute { get; init; }
}