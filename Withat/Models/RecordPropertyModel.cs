using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Withat;

public record RecordPropertyModel
{
    public string PropertyName { get; set; }
    public string PropertyTypeFQ { get; set; }
    public ImmutableArray<AttributeModel> Attributes { get; set; }
    public Accessibility SetAccessibility { get; set; }
}

public record AttributeModel
{
    public string AttributeName { get; set; }
    public ImmutableDictionary<string, object> Values { get; set; }
}