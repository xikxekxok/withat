using System;
using System.Collections.Generic;
using System.Linq;
using Withat.Models;

namespace Withat.SourceCode;

internal sealed class WithMethodEmitter
{
    // (async, receiverIsTask, valueKind) for the standard 8-overload family.
    private static readonly (bool Async, bool ReceiverIsTask, ValueKind ValueKind)[] EightOverloadConfigs =
    {
        (false, false, ValueKind.SyncValue),
        (true,  false, ValueKind.TaskValue),
        (false, false, ValueKind.SyncFunc),
        (true,  false, ValueKind.AsyncFunc),
        (true,  true,  ValueKind.SyncValue),
        (true,  true,  ValueKind.TaskValue),
        (true,  true,  ValueKind.SyncFunc),
        (true,  true,  ValueKind.AsyncFunc),
    };

    internal void EmitEightOverloads(
        System.Text.StringBuilder sb,
        RecordTypeModel rootType,
        PlannedPath plan,
        string methodBaseName,
        NullPropagationKind nullKind)
    {
        var t = plan.TargetTypeFq;
        var root = rootType.RecordTypeNameFull;

        foreach (var (async, receiverIsTask, valueKind) in EightOverloadConfigs)
        {
            var receiver = receiverIsTask ? $"Task<{root}> recordTask" : $"{root} record";
            var valueParam = valueKind switch
            {
                ValueKind.SyncValue  => $"{t} newValue",
                ValueKind.TaskValue  => $"Task<{t}> newValueTask",
                ValueKind.SyncFunc   => $"Func<{t},{t}> updateValueFunc",
                ValueKind.AsyncFunc  => $"Func<{t},Task<{t}>> updateValueFunc",
                _                    => throw new System.InvalidOperationException()
            };
            var sig = $"{methodBaseName}(this {receiver}, {valueParam})";
            EmitOneOverload(sb, rootType, plan, methodBaseName, sig, async, receiverIsTask, valueKind, nullKind);
        }
    }

    internal void EmitOneOverload(
        System.Text.StringBuilder sb,
        RecordTypeModel rootType,
        PlannedPath plan,
        string methodBaseName,
        string signatureLineWithoutModifier,
        bool async,
        bool receiverIsTask,
        ValueKind valueKind,
        NullPropagationKind nullKind)
    {
        var retType = async ? $"Task<{rootType.RecordTypeNameFull}>" : rootType.RecordTypeNameFull;
        var asyncKeyword = async ? "async " : "";

        sb.Append("    ");
        sb.AppendLine($"{plan.MethodModifier} static {asyncKeyword}{retType} {signatureLineWithoutModifier}");
        sb.AppendLine("    {");
        sb.AppendLine(Indent(BuildMethodBodyLines(plan, receiverIsTask, valueKind, nullKind, methodBaseName), 8));
        sb.AppendLine("    }");
    }

    private static string BuildMethodBodyLines(
        PlannedPath plan,
        bool receiverIsTask,
        ValueKind valueKind,
        NullPropagationKind nullKind,
        string methodDisplayName)
    {
        var path = plan.Segments;

        var lines = new List<string>();
        if (receiverIsTask)
            lines.Add("var record = await recordTask;");

        lines.AddRange(LoadRecordPrefixWithNullChecks(path, nullKind, methodDisplayName, plan.PathDescription));

        var assigned = valueKind switch
        {
            ValueKind.SyncFunc => $"updateValueFunc({BuildReadFromLoadedChain(path)})",
            ValueKind.AsyncFunc => $"await updateValueFunc({BuildReadFromLoadedChain(path)})",
            ValueKind.SyncValue => "newValue",
            ValueKind.TaskValue => "await newValueTask",
            _ => throw new ArgumentOutOfRangeException(nameof(valueKind), valueKind, null)
        };            
        lines.Add($"var __assigned = {assigned};");
        
        lines.AddRange(BuildReassignSuffix(path));

        return string.Join("\n", lines);
    }

    private static string BuildReadFromLoadedChain(IReadOnlyList<PathSegment> path)
    {
        var readVarIndex = path.Count - 1;
        var readExpr = $"v{readVarIndex}";
        return readExpr;
    }

    private static IEnumerable<string> LoadRecordPrefixWithNullChecks(
        IReadOnlyList<PathSegment> path,
        NullPropagationKind nullKind,
        string methodDisplayName,
        string pathDesc)
    {
        // Null propagation applies only to traversal nodes — segments we must pass
        // THROUGH to reach the target.  The target itself (last segment) is never
        // null-checked: we are replacing (or reading-to-transform) it, not traversing
        // through it, so its current value is irrelevant to the guard.

        var varName = "record";
        for (var i = 0; i < path.Count; i++)
        {
            var seg = path[i];
            
            var prevVarName = varName;
            varName = $"v{i}";
            yield return $"var {varName} = {prevVarName}.{seg.Name};";
            if (i != path.Count -1) //do not do it for last item
                foreach (var line in SegmentNullChecks(i, seg, nullKind, methodDisplayName, pathDesc, path))
                    yield return line;
        }
    }

    private static IEnumerable<string> BuildReassignSuffix(
        IReadOnlyList<PathSegment> path)
    {
        var containerIndex = path.Count - 2;
        yield return $"var u{containerIndex+1} = __assigned;";

        for (var i = containerIndex; i >= 0; i--)
        {
            var propName = path[i + 1].Name;
            yield return $"var u{i} = v{i} with {{ {propName} = u{i + 1} }};";
        }
        yield return $"return record with {{ {path[0].Name} = u0 }};";
    }

    private static IEnumerable<string> SegmentNullChecks(
        int segmentIndex,
        PathSegment seg,
        NullPropagationKind nullKind,
        string methodDisplayName,
        string pathDesc,
        IReadOnlyList<PathSegment> fullPath)
    {
        if (nullKind == NullPropagationKind.None || !seg.IsRecord || !seg.IsNullable)
            yield break;

        var pathToSeg = string.Join(".", fullPath.Take(segmentIndex + 1).Select(s => s.Name));
        var msg = NullThrowMessage(methodDisplayName, pathDesc, seg.Name, pathToSeg);
        var varName = $"v{segmentIndex}";
        if (nullKind == NullPropagationKind.Safe)
            yield return $"if ({varName} is null) return record;";
        else
            yield return $"if ({varName} is null) throw new NullReferenceException(\"{EscapeCsString(msg)}\");";
    }

    private static string NullThrowMessage(string methodDisplayName, string pathDesc, string segmentName, string pathPrefix) =>
        $"Withat: '{methodDisplayName}' cannot traverse a null record segment '{segmentName}' (path: {pathPrefix}). Full path: {pathDesc}.";

    private static string EscapeCsString(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string Indent(string s, int spaces)
    {
        var pad = new string(' ', spaces);
        return string.Join("\n", s.Split('\n').Where(l => l.Length > 0).Select(l => pad + l));
    }
}

