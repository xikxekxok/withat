using System.Collections.Immutable;

namespace Withat;

public record RecordTypeModel
{
    public string RecordTypeNameMinified { get; set; }
    public string RecordTypeNameFull { get; set; }
    public string RecordNamespaceName { get; set; }
    public ImmutableArray<RecordPropertyModel> Properties { get; set; }
}