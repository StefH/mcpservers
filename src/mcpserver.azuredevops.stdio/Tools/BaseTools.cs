﻿using System.Text.Json;

namespace ModelContextProtocolServer.AzureDevops.Stdio.Tools;

public abstract class BaseTools
{
    protected string ToJson(object value)
    {
        return JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
    }
}