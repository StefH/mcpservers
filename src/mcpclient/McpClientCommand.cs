using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client.Extensions;
using ModelContextProtocol.Client.Types;
using ModelContextProtocol.Configuration;
using ModelContextProtocol.Protocol.Transport;
using Spectre.Console;
using Spectre.Console.Cli;
using MenuItem = (string Menu, ModelContextProtocol.Client.McpClientTool? Tool);

namespace ModelContextProtocol.Client;

internal class McpClientCommand : AsyncCommand<McpClientCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--logger")]
        [Description("Logger (console or null)")]
        [DefaultValue("console")]
        public required string Logger { get; init; }

        [CommandOption("-t|--transportType")]
        [Description("MCP Server TransportType (stdio or sse)")]
        [DefaultValue(TransportTypes.StdIo)]
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
            if (TransportType != TransportTypes.StdIo && TransportType != TransportTypes.Sse)
            {
                return ValidationResult.Error("The transportType should be 'stdio' or 'sse'.");
            }

            if (Logger != "console" && Logger != "null")
            {
                return ValidationResult.Error("The logger should be 'console' or 'null'.");
            }

            if (TransportType == TransportTypes.StdIo && string.IsNullOrWhiteSpace(Command))
            {
                return ValidationResult.Error("The command is required for stdio.");
            }

            if (TransportType == TransportTypes.Sse && string.IsNullOrWhiteSpace(Location))
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

        var config = new McpServerConfig
        {
            Id = name.ToLowerInvariant(),
            Name = name,
            TransportType = settings.TransportType,
            Location = settings.Location,
            TransportOptions = []
        };

        if (settings.TransportType == TransportTypes.StdIo)
        {
            config.TransportOptions["command"] = settings.Command ?? throw new ArgumentNullException(nameof(settings.Command));
            if (settings.Arguments != null)
            {
                var arguments = settings.Arguments.Where(a => !a.StartsWith("env:")).ToArray();
                if (arguments.Length > 0)
                {
                    config.TransportOptions["arguments"] = string.Join(" ", arguments);
                }

                var environmentVariables = settings.Arguments.Where(a => a.StartsWith("env:")).ToArray();
                foreach (var e in environmentVariables.Select(e => e.Split("=")).Select(x => new { name = x[0], value = x[1] }))
                {
                    config.TransportOptions[e.name] = e.value;
                }
            }
        }

        var options = new McpClientOptions
        {
            ClientInfo = new() { Name = applicationName, Version = version }
        };

        var loggerFactory = string.Equals(settings.Logger, "console", StringComparison.OrdinalIgnoreCase) ?
            LoggerFactory.Create(c => c.AddConsole()) :
            NullLoggerFactory.Instance;

        var client = await McpClientFactory.CreateAsync(config, options, null, loggerFactory);
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

            var inputSchema = JsonSerializer.Deserialize<JsonSchema>(tool!.JsonSchema.GetRawText());
            var arguments = ArgumentUtils.GetArgumentValues(inputSchema?.Properties, inputSchema?.Required);
            var result = await tool.InvokeAsync(arguments);
            var text = ((JsonElement)result!).GetProperty("content")[0].GetProperty("text").GetString();

            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine($"Result: {text}");
            AnsiConsole.WriteLine();
        } while (true);

        return 0;
    }
}