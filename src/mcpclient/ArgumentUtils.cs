using System.Text.Json;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client.Types;
using Spectre.Console;

namespace ModelContextProtocol.Client;

internal static class ArgumentUtils
{
    public static AIFunctionArguments GetArgumentValues(Dictionary<string, JsonSchemaProperty>? properties, List<string>? required)
    {
        var arguments = new Dictionary<string, object?>();
        if (properties == null)
        {
            return new AIFunctionArguments();
        }

        var requiredPropertyNames = required ?? [];
        foreach (var (propertyName, property) in properties)
        {
            var description = property.Description ?? string.Empty;
            var isRequired = requiredPropertyNames.Contains(propertyName);
            var type = ConvertParameterDataType(property, isRequired);

            string value;
            if (type.Simple)
            {
                value = isRequired ?
                    AnsiConsole.Ask<string>($"Enter required value for {description} '{propertyName}' :") :
                    AnsiConsole.Ask($"Enter optional value for {description} '{propertyName}' :", "null");
            }
            else
            {
                value = isRequired ?
                    AnsiConsole.Ask<string>($"Enter required value for {description} '{propertyName}' as JSON: ") :
                    AnsiConsole.Ask($"Enter optional value for {description} '{propertyName}' as JSON :", "null");
            }

            arguments[propertyName] = ToArgumentValue(type.Type, value);
        }

        return new AIFunctionArguments(arguments);
    }

    private static (bool Simple, Type Type) ConvertParameterDataType(JsonSchemaProperty property, bool required)
    {
        (bool Simple, Type Type) type = property.Type switch
        {
            "string" => (true, typeof(string)),
            "integer" => (true, typeof(int)),
            "number" => (true, typeof(double)),
            "boolean" => (true, typeof(bool)),
            "array" => (false, typeof(List<object>)),
            // "object" => (false, typeof(Dictionary<string, object?>)),
            _ => (false, typeof(Dictionary<string, object?>))
        };

        return (type.Simple, !required && type.Type.IsValueType ? typeof(Nullable<>).MakeGenericType(type.Type) : type.Type);
    }

    private static object? ToArgumentValue(Type parameterType, string value)
    {
        if (value == "null")
        {
            return null;
        }

        if (Nullable.GetUnderlyingType(parameterType) == typeof(string))
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

        return JsonSerializer.Deserialize(value, parameterType);
    }
}