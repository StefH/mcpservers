using ModelContextProtocol.Client;
using Spectre.Console.Cli;

var commandApp = new CommandApp<McpClientCommand>();
return await commandApp.RunAsync(args);