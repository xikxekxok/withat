namespace Withat.Models;

public record RecordTypeModel
{
    public required string RecordTypeNameMinified { get; init; }
    public required string RecordTypeNameFull { get; init; }
    public required string RecordNamespaceName { get; init; }
    public required EquatableArray<RecordPropertyModel> Properties { get; init; }
}