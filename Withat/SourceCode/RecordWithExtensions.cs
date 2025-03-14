﻿using System.Linq;
using Microsoft.CodeAnalysis;
using Withat.Models;

namespace Withat.SourceCode;

internal static class RecordWithExtensions
{
    internal static string Generate(RecordTypeModel type) => $@"
// <auto-generated/>
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace {type.RecordNamespaceName};
public static class {type.RecordTypeNameMinified}_WithExtensions
{{
    {string.Join("", type.Properties.Select(x=>PropMethods(type,x)))}
}}
";
    
    
    
    private static string PropMethods(RecordTypeModel type, RecordPropertyModel property)
    {
        if (property.HasIgnoreAttribute)
            return "";
        var methodModifier = property.SetAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            _ => null
        };
        if (methodModifier == null)
            return "";
        var s = $@"
    {methodModifier} static {type.RecordTypeNameFull} With{property.PropertyName}(this {type.RecordTypeNameFull} record, {property.PropertyTypeFQ} new{property.PropertyName})
    {{
        return record with {{ {property.PropertyName} = new{property.PropertyName} }};
    }}

    {methodModifier} static async Task<{type.RecordTypeNameFull}> With{property.PropertyName}(this {type.RecordTypeNameFull} record, Task<{property.PropertyTypeFQ}> new{property.PropertyName}Task)
    {{
        return record with {{ {property.PropertyName} = await new{property.PropertyName}Task }};
    }}

    {methodModifier} static {type.RecordTypeNameFull} With{property.PropertyName}(this {type.RecordTypeNameFull} record, Func<{property.PropertyTypeFQ},{property.PropertyTypeFQ}> new{property.PropertyName}Func)
    {{
        return record with {{ {property.PropertyName} = new{property.PropertyName}Func(record.{property.PropertyName}) }};
    }}

    {methodModifier} static async Task<{type.RecordTypeNameFull}> With{property.PropertyName}(this {type.RecordTypeNameFull} record, Func<{property.PropertyTypeFQ},Task<{property.PropertyTypeFQ}>> new{property.PropertyName}Func)
    {{
        return record with {{ {property.PropertyName} = await new{property.PropertyName}Func(record.{property.PropertyName}) }};
    }}

    {methodModifier} static async Task<{type.RecordTypeNameFull}> With{property.PropertyName}(this Task<{type.RecordTypeNameFull}> recordTask, {property.PropertyTypeFQ} new{property.PropertyName})
    {{
        var record = await recordTask;
        return record with {{ {property.PropertyName} = new{property.PropertyName} }};
    }}

    {methodModifier} static async Task<{type.RecordTypeNameFull}> With{property.PropertyName}(this Task<{type.RecordTypeNameFull}> recordTask, Task<{property.PropertyTypeFQ}> new{property.PropertyName}Task)
    {{
        var record = await recordTask;
        return record with {{ {property.PropertyName} = await new{property.PropertyName}Task }};
    }}


    {methodModifier} static async Task<{type.RecordTypeNameFull}> With{property.PropertyName}(this Task<{type.RecordTypeNameFull}> recordTask, Func<{property.PropertyTypeFQ},{property.PropertyTypeFQ}> new{property.PropertyName}Func)
    {{
        var record = await recordTask;
        return record with {{ {property.PropertyName} = new{property.PropertyName}Func(record.{property.PropertyName}) }};
    }}

    {methodModifier} static async Task<{type.RecordTypeNameFull}> With{property.PropertyName}(this Task<{type.RecordTypeNameFull}> recordTask, Func<{property.PropertyTypeFQ},Task<{property.PropertyTypeFQ}>> new{property.PropertyName}Func)
    {{
        var record = await recordTask;
        return record with {{ {property.PropertyName} = await new{property.PropertyName}Func(record.{property.PropertyName}) }};
    }}
        ";
        return s;
    }
}