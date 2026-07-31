// Copyright (c) https://github.com/PederHP/mcpdotnet

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModelContextProtocol.Client.Types;

/// <summary>
/// Represents a property in a JSON schema.
/// <see href="https://github.com/modelcontextprotocol/specification/blob/main/schema/2024-11-05/schema.json">See the schema for details</see>
/// </summary>
internal class JsonSchemaProperty
{
    /// <summary>
    /// The type of the property. Should be a JSON Schema type and is required.
    /// </summary>
    [JsonPropertyName("type")]
    public required JsonElement Type { get; set; }

    /// <summary>
    /// A human-readable description of the property.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Map of property names to property definitions.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, JsonSchemaProperty>? Properties { get; set; }

    /// <summary>
    /// List of required property names.
    /// </summary>
    [JsonPropertyName("required")]
    public List<string>? Required { get; set; }


    /// <summary>
    /// The default value for the schema [optional].
    /// </summary>
    [JsonPropertyName("default")]
    public object? Default { get; set; }
}