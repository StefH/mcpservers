using ModelContextProtocolServer.Interceptor;
using Spectre.Console.Cli;

await new CommandApp<McpInterceptorCommand>().RunAsync(args);