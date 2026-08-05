using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Protocol;

namespace ModelContextProtocolServer.Stdio;

public static class StdioServer
{
    public static Task RunAsync(params string[] args)
    {
        return RunAsync((_, _) => { }, args);
    }

    public static Task RunAsync(Action<IServiceCollection> servicesAction, params string[] args)
    {
        return RunAsync((services, _) => servicesAction(services), args);
    }

    public static Task RunAsync(Action<IServiceCollection, IConfiguration> action, params string[] args)
    {
        var options = new StdioServerOptions();
        return RunAsync(options, action, args);
    }

    public static Task RunAsync(string applicationName, string version, Action<IServiceCollection, IConfiguration> servicesAction, params string[] args)
    {
        var options = new StdioServerOptions
        {
            Name = applicationName,
            Version = version
        };

        return RunAsync(options: options, servicesAction: servicesAction, args);
    }

    public static Task RunAsync(StdioServerOptions options, Action<IServiceCollection, IConfiguration> servicesAction, params string[] args)
    {
        var builder = Host.CreateEmptyApplicationBuilder(settings: new HostApplicationBuilderSettings
        {
            ApplicationName = options.Name,
            Args = args
        });

        builder.Configuration
            .AddCommandLine(args)
            .AddEnvironmentVariables();

        builder.Services
            .AddSingleton(LoggerHelper.CreateLoggerFactory(options.Name))
            .AddMcpServer(o => 
            {
                o.ServerInfo = new Implementation
                {
                    Name = options.Name,
                    Version = options.Version
                };

                o.ServerInstructions = options.ServerInstructions;
            })
            .WithStdioServerTransport()
            .WithToolsFromAssembly(Assembly.GetEntryAssembly());

        servicesAction(builder.Services, builder.Configuration);

        var host = builder.Build();

        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            cts.Cancel();
        };

        return host.RunAsync(cts.Token);
    }
}
