using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Withat.Models;

namespace Withat.SourceCode;

internal enum NullPropagationKind
{
    None,
    Safe,
    Throw
}

internal enum ValueKind
{
    SyncValue,
    TaskValue,
    SyncFunc,
    AsyncFunc
}

internal sealed record PathSegment(
    string Name,
    string TypeFq,
    bool IsRecord,
    bool IsNullable,
    bool InternalSet);

internal sealed record PlannedPath(
    IReadOnlyList<PathSegment> Segments)
{
    public string PathSuffix => string.Join("_", Segments.Select(p => p.Name));
    public string PathDescription => string.Join(".", Segments.Select(p => p.Name));
    public bool HasNullableRecordSegment => Segments.Any(s => s.IsRecord && s.IsNullable);
    public string TargetTypeFq => Segments[^1].TypeFq;
    public string MethodModifier => Segments.Any(x => x.InternalSet) ? "internal" : "public";
}

