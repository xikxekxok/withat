using Microsoft.CodeAnalysis;

namespace Withat.Models;

public record RecordPropertyModel
{
    public required string PropertyName { get; init; }
    public required string PropertyTypeFQ { get; init; }
    public required Accessibility? SetAccessibility { get; init; }
    public required bool HasIgnoreAttribute { get; init; }
    public required bool HasNoNestedWithAttribute { get; init; }
    public required bool IsRecord { get; init; }
    public required bool IsNullable { get; init; }
    public required EquatableArray<RecordPropertyModel> NestedProperties { get; init; }
}