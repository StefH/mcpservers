namespace ModelContextProtocolServer.Sse;

internal class ArgsParser
{
    private const string Sigil = "--";

    private IDictionary<string, string[]> Arguments { get; } = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

    public void Parse(string[] arguments)
    {
        var currentName = string.Empty;

        var values = new List<string>();

        foreach (var arg in arguments.SelectMany(arg => arg.Split(' ')))
        {
            if (arg.StartsWith(Sigil))
            {
                if (!string.IsNullOrEmpty(currentName))
                {
                    Arguments[currentName] = values.ToArray();
                }

                values.Clear();
                currentName = arg.Substring(Sigil.Length);
            }
            else if (string.IsNullOrEmpty(currentName))
            {
                Arguments[arg] = [];
            }
            else
            {
                values.Add(arg);
            }
        }

        if (!string.IsNullOrEmpty(currentName))
        {
            Arguments[currentName] = values.ToArray();
        }
    }

    public bool Contains(string name)
    {
        return Arguments.ContainsKey(name);
    }

    public string[]? GetValues(string name)
    {
        return Contains(name) ? Arguments[name] : default;
    }

    public T GetValue<T>(string name, Func<string[], T> func, T defaultValue)
    {
        return Contains(name) ? func(Arguments[name]) : defaultValue;
    }

    public int? GetIntValue(string name)
    {
        return GetValue(name, values =>
        {
            var value = values.FirstOrDefault();
            return int.TryParse(value, out var intValue) ? intValue : (int?)null;
        }, null);
    }

    public int[]? GetIntValues(string name)
    {
        var values = GetValues(name);
        if (values == null)
        {
            return null;
        }

        var integers = new List<int>();

        foreach (var value in values)
        {
            if (int.TryParse(value, out var intValue))
            {
                integers.Add(intValue);
            }
        }

        return integers.ToArray();
    }
}