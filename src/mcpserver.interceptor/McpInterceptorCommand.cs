using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocolServer.Stdio;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ModelContextProtocolServer.Interceptor;

internal sealed class McpInterceptorCommand : AsyncCommand<McpInterceptorCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--logger")]
        [Description("Logger (console or null)")]
        [DefaultValue("console")]
        public required string Logger { get; init; }

        [CommandOption("-s|--serverName")]
        [Description("MCP Server Name")]
        public required string Servername { get; init; }

        [CommandOption("-c|--command")]
        [Description("MCP Server Command with Arguments (stdio only)")]
        public required string CommandWithArguments { get; init; }

        [CommandOption("-e|--env")]
        [Description("MCP Server Environment variables (stdio only)")]
        public string[] EnvironmentVariables { get; init; } = [];

        public string Command { get; private set; } = null!;

        public string[] Arguments { get; private set; } = [];

        public Dictionary<string, string?> EnvironmentVariablesAsDictionary { get; private set; } = [];

        public override ValidationResult Validate()
        {
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }

            if (Logger != "console" && Logger != "null")
            {
                return ValidationResult.Error("The logger should be 'console' or 'null'.");
            }

            if (string.IsNullOrWhiteSpace(Servername))
            {
                return ValidationResult.Error("The servername is required.");
            }

            if (string.IsNullOrWhiteSpace(CommandWithArguments))
            {
                return ValidationResult.Error("The command is required for stdio.");
            }

            var commandWithArguments = CommandWithArguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Command = commandWithArguments[0];

            if (commandWithArguments.Length > 1)
            {
                var arguments = commandWithArguments.Skip(1).Where(a => !a.StartsWith("env:")).ToArray();
                if (arguments.Length > 0)
                {
                    Arguments = arguments;
                }

                var environmentVariables = commandWithArguments.Skip(1).Where(a => a.StartsWith("env:")).ToArray();
                foreach (var e in environmentVariables.Select(e => e.Split('=', 2)).Select(x => new { name = x[0], value = x[1] }))
                {
                    EnvironmentVariablesAsDictionary[e.name] = e.value;
                }

                foreach (var e in EnvironmentVariables)
                {
                    var parts = e.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        EnvironmentVariablesAsDictionary[parts[0]] = parts[1];
                    }
                }
            }

            return ValidationResult.Success();
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetEntryAssembly();
        var assemblyTitle = assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? $"mcpserver.{Guid.NewGuid()}.stdio";
        var name = $"{assemblyTitle} (interceptor-for-{settings.Servername})";
        var version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "1.0.0";

        var builder = Host.CreateEmptyApplicationBuilder(settings: new HostApplicationBuilderSettings
        {
            ApplicationName = name,
            Args = settings.Arguments
        });

        var stdioOptions = new StdioClientTransportOptions
        {
            Command = settings.Command,
            Arguments = settings.Arguments,
            EnvironmentVariables = settings.EnvironmentVariablesAsDictionary,
            Name = name
        };

        var clientTransport = new StdioClientTransport(stdioOptions, null);

        var clientOptions = new McpClientOptions
        {
            ClientInfo = new()
            {
                Name = name,
                Version = version
            }
        };

        var loggerFactory = LoggerHelper.CreateLoggerFactory(name);
        var logger = loggerFactory.CreateLogger<McpInterceptorCommand>();

        var client = await McpClient.CreateAsync(clientTransport, clientOptions, loggerFactory, cancellationToken);

        builder.Configuration
            .AddCommandLine(settings.Arguments)
            .AddEnvironmentVariables();

        builder.Services
            .AddSingleton(loggerFactory)
            .AddMcpServer(async o =>
            {
                o.ServerInfo = new Implementation
                {
                    Name = name,
                    Version = version
                };

                o.Handlers = new HandlersBuilder(logger, client).Build();
            }
        )
        .WithStdioServerTransport();

        var host = builder.Build();

        await host.RunAsync(cancellationToken);

        await client.DisposeAsync();

        return 0;
    }
}