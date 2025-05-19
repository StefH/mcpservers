using System.ComponentModel;
using ModelContextProtocol.Server;
using ModelContextProtocolServer.Everything.Stdio.Models;

namespace ModelContextProtocolServer.Everything.Stdio.Tools;

[McpServerToolType]
public static class EverythingTools
{
    [McpServerTool, Description("Echoes back the input")]
    public static string Echo([Description("Message to echo")] string message)
    {
        return message;
    }

    [McpServerTool, Description("Adds two numbers")]
    public static string Add([Description("First number")] double a, [Description("Second number")] double b)
    {
        var sum = a + b;
        return $"The sum of {a} and {b} is {sum}.";
    }

    [McpServerTool, Description("Adds two complex numbers")]
    public static string AddComplex([Description("First complex number")] ComplexNumber complexA, [Description("Second complex number")] ComplexNumber complexB)
    {
        var sum = new ComplexNumber(complexA.Real + complexB.Real, complexA.Imaginary + complexB.Imaginary);
        return $"The sum of {complexA} and {complexB} is {sum}.";
    }
}