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
        return RunAsync((_, _) => { }, args);
    }

    public static Task RunAsync(Action<IServiceCollection> servicesAction, params string[] args)
    {
        return RunAsync((services, _) => servicesAction(services), args);
    }

    public static Task RunAsync(Action<IServiceCollection, IConfiguration> action, params string[] args)
    {
        var options = new SseServerOptions();
        return RunAsync(options, action, args);
    }

    public static Task RunAsync(string applicationName, string version, Action<IServiceCollection, IConfiguration> servicesAction, params string[] args)
    {
        var options = new SseServerOptions
        {
            Name = applicationName,
            Version = version
        };

        return RunAsync(options, servicesAction, args);
    }

    public static Task RunAsync(SseServerOptions options, Action<IServiceCollection, IConfiguration> servicesAction, params string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = options.Name,
            Args = args
        });

        builder.Services
            .AddSingleton(LoggerHelper.CreateLoggerFactory(options.Name, true))
            .AddMcpServer(o => o.ServerInfo = new Implementation
            {
                Name = options.Name,
                Version = options.Version
            })
            .WithHttpTransport()
            .WithToolsFromAssembly(Assembly.GetEntryAssembly());

        // AddAuthentication TODO
        //builder.Services.AddAuthentication(options =>
        //{

        //})
        //.AddMcp();

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
