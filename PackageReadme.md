﻿# ModelContextProtocol-SemanticKernel
[Microsoft SemanticKernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/) integration for the [Model Context Protocol](https://modelcontextprotocol.io) using the [csharp-sdk](https://github.com/modelcontextprotocol/csharp-sdk).
Enables seamless use of MCP tools as AI functions.

## 💻 Stdio Example
### Code
``` csharp
var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));

builder.Services.AddOpenAIChatCompletion(
    serviceId: "openai",
    modelId: "gpt-4o-mini",
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!);

var kernel = builder.Build();

var transportOptions = new Dictionary<string, string>
{
    ["command"] = "npx",
    ["arguments"] = "-y --verbose @modelcontextprotocol/server-everything"
};
// 💡 Add this line to enable MCP functions from a Stdio server named "Everything"
await kernel.Plugins.AddMcpFunctionsFromStdioServerAsync("Everything", transportOptions);

var executionSettings = new OpenAIPromptExecutionSettings
{
    Temperature = 0,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var prompt = "Please call the echo tool with the string 'Hello Stef!' and give me the response as-is.";
var result = await kernel.InvokePromptAsync(prompt, new(executionSettings)).ConfigureAwait(false);
Console.WriteLine($"\n\n{prompt}\n{result}");
```

### Result
```
Please call the echo tool with the string 'Hello Stef!' and give me the response as-is.
Echo: Hello Stef!
```


## 💻 SSE (Server Side Events) Example
### Code
``` csharp
var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));

builder.Services.AddOpenAIChatCompletion(
    serviceId: "openai",
    modelId: "gpt-4o-mini",
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!);

var kernel = builder.Build();

// 💡 Add this line to enable MCP functions from a Sse server named "Github"
// - Note that a server must be running at the specified URL
await kernel.Plugins.AddMcpFunctionsFromSseServerAsync("GitHub", "http://localhost:12345");

var executionSettings = new OpenAIPromptExecutionSettings
{
    Temperature = 0,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var prompt = "Summarize the last 3 commits to the StefH/FluentBuilder repository?";
var result = await kernel.InvokePromptAsync(prompt, new(executionSettings)).ConfigureAwait(false);
Console.WriteLine($"\n\n{prompt}\n{result}");
```

### Result
```
Summarize the last 3 commits to the StefH/FluentBuilder repository?
Here are the summaries of the last three commits to the `StefH/FluentBuilder` repository:

1. **Commit [2293880](https://github.com/StefH/FluentBuilder/commit/229388090f50a39f489e30cb535f67f3705cf61f)** (January 30, 2025)
   - **Author:** Stef Heyenrath
   - **Message:** Update README.md
   - **Details:** This commit updates the README.md file. The commit was verified and is valid.

2. **Commit [ae27064](https://github.com/StefH/FluentBuilder/commit/ae2706424c3b75613bf5625091aa2649fb33ecde)** (November 6, 2024)
   - **Author:** Stef Heyenrath
   - **Message:** Update README.md
   - **Details:** This commit also updates the README.md file. The commit was verified and is valid.

3. **Commit [53096a8](https://github.com/StefH/FluentBuilder/commit/53096a8b54a1029532425bc727fdd831e9ed0092)** (October 20, 2024)
   - **Author:** Stef Heyenrath
   - **Message:** Update README.md
   - **Details:** This commit updates the README.md file as well. The commit was verified and is valid.

All three commits involve updates to the README.md file, reflecting ongoing improvements or changes to the documentation.
```