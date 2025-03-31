using ModelContextProtocol.Client.Types;
using Spectre.Console;

namespace ModelContextProtocol.Client;

internal static class ArgumentUtils
{
    public static Dictionary<string, object?> GetArgumentValues(Dictionary<string, JsonSchemaProperty>? properties, List<string>? required)
    {
        var arguments = new Dictionary<string, object?>();
        if (properties == null)
        {
            return arguments;
        }

        var requiredPropertyNames = required ?? [];
        foreach (var (propertyName, property) in properties)
        {
            var description = property.Description ?? string.Empty;
            var isRequired = requiredPropertyNames.Contains(propertyName);

            var value = isRequired ? 
                AnsiConsole.Ask<string>($"Enter required value for {description} '{propertyName}' :"):
                AnsiConsole.Ask($"Enter optional value for {description} '{propertyName}' :", "null");

            var type = ConvertParameterDataType(property, isRequired);
            arguments[propertyName] = ToArgumentValue(type, value);
        }

        return arguments;
    }

    private static Type ConvertParameterDataType(JsonSchemaProperty property, bool required)
    {
        var type = property.Type switch
        {
            "string" => typeof(string),
            "integer" => typeof(int),
            "number" => typeof(double),
            "boolean" => typeof(bool),
            "array" => typeof(List<string>),
            "object" => typeof(Dictionary<string, object>),
            _ => typeof(object)
        };

        return !required && type.IsValueType ? typeof(Nullable<>).MakeGenericType(type) : type;
    }

    private static object? ToArgumentValue(Type parameterType, string value)
    {
        if (value == "null")
        {
            return null;
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

        //if (parameterType == typeof(List<string>))
        //{
        //    return (value as IEnumerable<object>)?.ToList() ?? value;
        //}

        //if (parameterType == typeof(Dictionary<string, object>))
        //{
        //    return (value as Dictionary<string, object>)?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? value;
        //}

        return value;
    }
}