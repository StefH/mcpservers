using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.SemanticKernel.Extensions;

var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

using var cts = new CancellationTokenSource();

var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));

builder.Services.AddOpenAIChatCompletion(
    serviceId: "openai",
    modelId: "gpt-4o",
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!
);

var kernel = builder.Build();

//await kernel.Plugins.AddMcpFunctionsFromStdioServerAsync(
//    "MCPInterceptor for Everything",
//    "dotnet",
//    [
//        "run",
//        "--project",
//        @"C:\dev\GitHub\mcpservers\src\mcpserver.interceptor\mcpserver.interceptor.csproj",
//        "--serverName=Everything",
//        "--command=dnx mcpserver.everything.stdio --yes"
//    ],
//    cancellationToken: cts.Token);

await kernel.Plugins.AddMcpFunctionsFromStdioServerAsync(
    "MCPInterceptor for AzureDevOps",
    "dotnet",
    [
        "run",
        "--project",
        @"C:\dev\GitHub\mcpservers\src\mcpserver.interceptor\mcpserver.interceptor.csproj",
        "--serverName=AzureDevOps",
        "--command=dnx mcpserver.azuredevops.stdio --yes env:_X_=test",
        "--env=AZURE_DEVOPS_ORG_URL=https://dev.azure.com/alfa1group",
        "--env=AZURE_DEVOPS_AUTH_METHOD=pat",
        $"--env=AZURE_DEVOPS_PAT={Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT")}"
    ],
    //new Dictionary<string, string>
    //{
    //    { "_X_", "test" },
    //    { "AZURE_DEVOPS_ORG_URL", "https://dev.azure.com/alfa1group" },
    //    { "AZURE_DEVOPS_AUTH_METHOD", "pat" },
    //    { "AZURE_DEVOPS_PAT", Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT")! }
    //},
    cancellationToken: cts.Token);

var executionSettings = new OpenAIPromptExecutionSettings
{
    Temperature = 0.1,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var logger = kernel.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");

//var result = await kernel.InvokePromptAsync("Which tools are currently registered? And what are the functions?", new(executionSettings));
//Console.WriteLine($"\n\nTools:\n{result}");

//var promptComplex = "Use the Everything tool and call the add_complex function to add these complex numbers: 1 + 2i and 3 - 7i";
//var resultComplex = await kernel.InvokePromptAsync(promptComplex, new(executionSettings));
//Console.WriteLine($"\n\n{promptComplex}\n{resultComplex}");

var promptAzureDevops =
    """
    For the Azure Devops project 'mstack-skills' and repository 'mstack-skills-blazor', get 2 latest commits with all details.
    """;
var resultAzureDevops = await kernel.InvokePromptAsync(promptAzureDevops, new(executionSettings));
Console.WriteLine($"\n\n{promptAzureDevops}\n{resultAzureDevops}");

await cts.CancelAsync();