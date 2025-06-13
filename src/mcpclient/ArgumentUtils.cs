using System.Text.Json;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client.Types;
using Spectre.Console;

namespace ModelContextProtocol.Client;

internal static class ArgumentUtils
{
    public static AIFunctionArguments GetArgumentValues(int level, Dictionary<string, JsonSchemaProperty>? properties, List<string>? required)
    {
        var arguments = new Dictionary<string, object?>();
        if (properties == null)
        {
            return new AIFunctionArguments();
        }

        var spaces = level > 0 ? new string(' ', level) : string.Empty;

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
                    AnsiConsole.Ask<string>($"{spaces}Enter required value for {description} '{propertyName}' : ") :
                    AnsiConsole.Ask($"{spaces}Enter optional value for {description} '{propertyName}' : ", "null");
            }
            else
            {
                bool defineValue;
                if (isRequired)
                {
                    AnsiConsole.WriteLine($"{spaces}Enter required value for {description} '{propertyName}': ");
                    defineValue = true;
                }
                else
                {
                    defineValue = AnsiConsole.Confirm($"Do you want to define an optional value for {description} '{propertyName}'?", false);
                }

                if (defineValue)
                {
                    if (type.Type == typeof(Dictionary<string, object?>))
                    {
                        var args = GetArgumentValues(level + 1, property.Properties, property.Required);
                        value = JsonSerializer.Serialize(args);
                    }
                    else
                    {
                        var num = AnsiConsole.Ask<int>($"{spaces}How many array items?");
                        var array = Enumerable.Range(0, num).Select(index => AnsiConsole.Ask($"{spaces}Enter value for array item[{index}] :", "null"));
                        value = JsonSerializer.Serialize(array);
                    }

                    AnsiConsole.WriteLine();
                }
                else
                {
                    value = "null";
                }
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

    private static object? ToArgumentValue(Type parameterType, object? value)
    {
        if (value is null or "null")
        {
            return null;
        }

        if (value is string stringValue)
        {
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

            return JsonSerializer.Deserialize(stringValue, parameterType);
        }

        return value;
    }
}