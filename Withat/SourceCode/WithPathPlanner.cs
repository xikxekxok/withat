using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Withat.Models;

namespace Withat.SourceCode;

internal static class WithPathPlanner
{
    internal static IEnumerable<PlannedPath> Plan(RecordTypeModel type)
    {
        foreach (var path in EnumerateAllUpdatePaths(type))
        {
            yield return new PlannedPath(path);
        }
    }


    private static IEnumerable<PathSegment[]> EnumerateAllUpdatePaths(RecordTypeModel type) =>
        VisitProperties(prefix: new List<PathSegment>(), properties: type.Properties);

    private static PathSegment ToSegment(RecordPropertyModel prop) =>
        new(prop.PropertyName, prop.PropertyTypeFQ, prop.IsRecord, prop.IsNullable, prop.SetAccessibility == Accessibility.Internal);

    private static IEnumerable<PathSegment[]> VisitProperties(
        List<PathSegment> prefix,
        IEnumerable<RecordPropertyModel> properties)
    {
        foreach (var prop in properties)
        {
            if (prop.HasIgnoreAttribute)
                continue;
            if (prop.SetAccessibility is not (Accessibility.Public or Accessibility.Internal))
                continue;

            var seg = ToSegment(prop);
            prefix.Add(seg);

            // Leaf update (for non-record) or top-level record update (path length 1).
            yield return prefix.ToArray();
            
            if (!prop.HasNoNestedWithAttribute && !prop.NestedProperties.IsDefaultOrEmpty)
            {
                foreach (var x in VisitProperties(prefix, prop.NestedProperties))
                    yield return x;
            }
            prefix.RemoveAt(prefix.Count - 1);
        }
    }
}

