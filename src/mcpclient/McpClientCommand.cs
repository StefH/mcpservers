using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client.Extensions;
using ModelContextProtocol.Client.Types;
using Spectre.Console;
using Spectre.Console.Cli;
using MenuItem = (string Menu, ModelContextProtocol.Client.McpClientTool? Tool);

namespace ModelContextProtocol.Client;

internal class McpClientCommand : AsyncCommand<McpClientCommand.Settings>
{
    private const string TransportTypeStdIo = "stdio";
    private const string TransportTypeSse = "sse";

    public class Settings : CommandSettings
    {
        [CommandOption("--logger")]
        [Description("Logger (console or null)")]
        [DefaultValue("console")]
        public required string Logger { get; init; }

        [CommandOption("-t|--transportType")]
        [Description("MCP Server TransportType (stdio or sse)")]
        [DefaultValue(TransportTypeStdIo)]
        public required string TransportType { get; init; }

        [CommandOption("-l|--location")]
        [Description("MCP Server Location (sse only)")]
        public string? Location { get; init; }

        [CommandOption("-c|--command")]
        [Description("MCP Server Command (stdio only)")]
        public string? Command { get; init; }

        [CommandArgument(0, "[arguments]")]
        [Description("MCP Server Arguments (stdio only)")]
        public string[]? Arguments { get; init; }

        public override ValidationResult Validate()
        {
            if (TransportType != TransportTypeStdIo && TransportType != TransportTypeSse)
            {
                return ValidationResult.Error("The transportType should be 'stdio' or 'sse'.");
            }

            if (Logger != "console" && Logger != "null")
            {
                return ValidationResult.Error("The logger should be 'console' or 'null'.");
            }

            if (TransportType == TransportTypeStdIo && string.IsNullOrWhiteSpace(Command))
            {
                return ValidationResult.Error("The command is required for stdio.");
            }

            if (TransportType == TransportTypeSse && string.IsNullOrWhiteSpace(Location))
            {
                return ValidationResult.Error("The location is required for sse.");
            }

            return ValidationResult.Success();
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var assembly = Assembly.GetEntryAssembly();
        var applicationName = assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? $"mcpclient.{Guid.NewGuid()}";
        var version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "1.0.0";
        var name = settings.Command ?? Guid.NewGuid().ToString();

        var clientOptions = new McpClientOptions
        {
            ClientInfo = new() { Name = applicationName, Version = version }
        };

        var loggerFactory = string.Equals(settings.Logger, "console", StringComparison.OrdinalIgnoreCase) ?
            LoggerFactory.Create(c => c.AddConsole()) :
            NullLoggerFactory.Instance;

        IClientTransport clientTransport;
        if (settings.TransportType == TransportTypeStdIo)
        {
            var stdioOptions = new StdioClientTransportOptions
            {
                Command = settings.Command ?? throw new ArgumentNullException(nameof(settings.Command)),
                EnvironmentVariables = new Dictionary<string, string?>(),
                Name = name
            };

            if (settings.Arguments != null)
            {
                var arguments = settings.Arguments.Where(a => !a.StartsWith("env:")).ToArray();
                if (arguments.Length > 0)
                {
                    stdioOptions.Arguments = arguments;
                }

                var environmentVariables = settings.Arguments.Where(a => a.StartsWith("env:")).ToArray();
                foreach (var e in environmentVariables.Select(e => e.Split("=")).Select(x => new { name = x[0], value = x[1] }))
                {
                    stdioOptions.EnvironmentVariables[e.name] = e.value;
                }
            }

            clientTransport = new StdioClientTransport(stdioOptions, loggerFactory);
        }
        else
        {
            var sseOptions = new SseClientTransportOptions
            {
                Endpoint = settings.Location != null ? new Uri(settings.Location) : throw new ArgumentException(nameof(settings.Location)),
                // AdditionalHeaders = TODO
                Name = name
            };
            clientTransport = new SseClientTransport(sseOptions, loggerFactory);
        }

        var client = await McpClientFactory.CreateAsync(clientTransport, clientOptions, loggerFactory: loggerFactory);
        client.DisposeAsyncOnApplicationExit();

        var tools = await client.ListToolsAsync();
        var prompts = tools
            .OrderBy(t => t.Name)
            .Select(t => new MenuItem($"{t.Name} ({t.Description})", t)).ToList();
        prompts.Add(new MenuItem("Quit", null));

        do
        {
            AnsiConsole.WriteLine();
            var (menu, tool) = AnsiConsole.Prompt(
                new SelectionPrompt<MenuItem>()
                    .Title("Select a tool to execute:")
                    .PageSize(10)
                    .AddChoices(prompts)
                    .UseConverter(menuItem => menuItem.Menu)
            );

            if (menu == "Quit")
            {
                break;
            }

            var inputSchema = tool!.JsonSchema.Deserialize<JsonSchema>();
            var arguments = ArgumentUtils.GetArgumentValues(0, inputSchema?.Properties, inputSchema?.Required);
            var result = await tool.InvokeAsync(arguments);
            var text = ((JsonElement)result!).GetProperty("content")[0].GetProperty("text").GetString();

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLineInterpolated($"[yellow]Result:[/] {text}");
            AnsiConsole.WriteLine();
        } while (true);

        return 0;
    }
}