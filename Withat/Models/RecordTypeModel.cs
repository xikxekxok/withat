using System.Collections.Immutable;

namespace Withat.Models;

public record RecordTypeModel
{
    public required string RecordTypeNameMinified { get; init; }
    public required string RecordTypeNameFull { get; init; }
    public required string RecordNamespaceName { get; init; }
    public required ImmutableArray<RecordPropertyModel> Properties { get; init; }
}