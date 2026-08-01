using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client.Types;
using Spectre.Console;

namespace ModelContextProtocol.Client;

internal static class ArgumentUtils
{
    public static AIFunctionArguments GetArgumentValues(int level, Dictionary<string, JsonSchemaProperty>? properties, List<string>? required)
    {
        var functionArguments = new AIFunctionArguments();
        if (properties == null)
        {
            return functionArguments;
        }

        var spaces = level > 0 ? new string(' ', level) : string.Empty;

        var requiredPropertyNames = required ?? [];
        foreach (var (propertyName, property) in properties)
        {
            var escapedDescription = (property.Description ?? string.Empty).EscapeMarkup();
            var escapedPropertyName = propertyName.EscapeMarkup();
            var isRequired = requiredPropertyNames.Contains(propertyName);
            var type = ConvertParameterDataType(property, isRequired);

            string value;
            if (type.Simple)
            {
                value = isRequired ?
                    AnsiConsole.Ask<string>($"{spaces}Enter required value for {escapedDescription} '{escapedPropertyName}' : ") :
                    AnsiConsole.Ask($"{spaces}Enter optional value for {escapedDescription} '{escapedPropertyName}' : ", property.Default?.ToString() ?? "null");
            }
            else
            {
                bool defineValue;

                if (isRequired)
                {
                    defineValue = true;
                }
                else
                {
                    defineValue = AnsiConsole.Confirm($"Do you want to define an optional value for {escapedDescription} '{escapedPropertyName}'?", false);
                }

                if (defineValue)
                {
                    if (type.Type == typeof(Dictionary<string, object?>))
                    {
                        if (property.Properties == null)
                        {
                            value = AnsiConsole.Ask<string>($"{spaces}Enter required value for {escapedDescription} '{escapedPropertyName}': ");
                        }
                        else
                        {
                            AnsiConsole.WriteLine($"{spaces}Enter required value for {escapedDescription} '{escapedPropertyName}': ");
                            var args = GetArgumentValues(level + 1, property.Properties, property.Required);
                            value = JsonSerializer.Serialize(args);
                        }
                    }
                    else
                    {
                        var num = AnsiConsole.Ask<int>($"{spaces}How many array items?");
                        var array = Enumerable.Range(0, num).Select(index => AnsiConsole.Ask($"{spaces}Enter value for array item[[{index}]] :", "null"));
                        value = JsonSerializer.Serialize(array);
                    }

                    AnsiConsole.WriteLine();
                }
                else
                {
                    value = "null";
                }
            }

            functionArguments.Add(propertyName, ToArgumentValue(type.Type, value));
        }

        return functionArguments;
    }

    private static (bool Simple, Type Type) ConvertParameterDataType(JsonSchemaProperty property, bool required)
    {
        string? type = null;
        Type? itemType = null;

        if (property.Type.ValueKind == JsonValueKind.String)
        {
            type = property.Type.GetString();
        }
        else if (property.Type.ValueKind == JsonValueKind.Array)
        {
            type = property.Type.Deserialize<string[]>()?.FirstOrDefault(x => !string.IsNullOrEmpty(x) && x != "null");

            if (property.Items != null)
            {
                itemType = ConvertParameterDataType(property.Items, true).Type;
            }
        }

        (bool Simple, Type Type) x = type switch
        {
            "string" => (true, typeof(string)),
            "integer" => (true, typeof(int)),
            "number" => (true, typeof(double)),
            "boolean" => (true, typeof(bool)),
            "array" => (false, typeof(List<>).MakeGenericType(itemType ?? typeof(object))),

            _ => (false, typeof(Dictionary<string, object?>))
        };

        return (x.Simple, !required && x.Type.IsValueType ? typeof(Nullable<>).MakeGenericType(x.Type) : x.Type);
    }

    private static object? ToArgumentValue(Type parameterType, object? value)
    {
        if (value is null or "null")
        {
            return null;
        }

        if (value is string stringValue)
        {
            if (parameterType == typeof(string) || Nullable.GetUnderlyingType(parameterType) == typeof(string))
            {
                return value;
            }

            if (Nullable.GetUnderlyingType(parameterType) == typeof(int))
            {
                return Convert.ToInt32(value);
            }

            if (Nullable.GetUnderlyingType(parameterType) == typeof(double))
            {
                return Convert.ToDouble(value);
            }

            if (Nullable.GetUnderlyingType(parameterType) == typeof(bool))
            {
                return Convert.ToBoolean(value);
            }

            return JsonSerializer.Deserialize(stringValue, parameterType);
        }

        return value;
    }
}