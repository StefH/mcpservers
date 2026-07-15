using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Protocol;
using ModelContextProtocolServer.Stdio;

namespace ModelContextProtocolServer.Sse;

public static class SseServer
{
    public static Task RunAsync(params string[] args)
    {
        return RunAsync((services, config) => { }, args);
    }

    public static Task RunAsync(Action<IServiceCollection, IConfiguration> servicesAction, params string[] args)
    {
        var assembly = Assembly.GetEntryAssembly();
        var applicationName = assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? $"mcpserver.{Guid.NewGuid()}.sse";
        var version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "1.0.0";

        return RunAsync(applicationName, version, servicesAction, args);
    }

    public static Task RunAsync(string applicationName, string version, Action<IServiceCollection, IConfiguration> servicesAction, params string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = applicationName,
            Args = args
        });

        builder.Services
            .AddSingleton(LoggerHelper.CreateLoggerFactory(applicationName))
            .AddMcpServer(o => o.ServerInfo = new Implementation
            {
                Name = applicationName,
                Version = version
            })
            .WithHttpTransport()
            .WithToolsFromAssembly(Assembly.GetEntryAssembly());

        builder.Services.AddAuthentication(options =>
        {

        })
        .AddMcp();

        builder.Configuration
            .AddCommandLine(args)
            .AddEnvironmentVariables();

        servicesAction(builder.Services, builder.Configuration);

        var app = builder.Build();
        app.MapMcp();

        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        AppDomain.CurrentDomain.ProcessExit += (_, e) =>
        {
            cts.Cancel();
        };

        return app.RunAsync(cts.Token);
    }
}