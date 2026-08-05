using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocolServer.Sse;
using ModelContextProtocolServer.Stdio;

namespace ModelContextProtocolServer.Hybrid;

public static class HybridServer
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
        var options = new HybridServerOptions();
        return RunAsync(options, action, args);
    }

    public static Task RunAsync(string applicationName, string version, Action<IServiceCollection, IConfiguration> servicesAction, params string[] args)
    {
        var options = new HybridServerOptions
        {
            Name = applicationName,
            Version = version
        };

        return RunAsync(options, servicesAction, args);
    }

    public static Task RunAsync(HybridServerOptions options, Action<IServiceCollection, IConfiguration> servicesAction, params string[] args)
    {
        if (args.Contains("--sse"))
        {
            var sseOptions = new SseServerOptions
            {
                Name = options.Name,
                Version = options.Version,
                ServerInstructions = options.ServerInstructions
            };
            return SseServer.RunAsync(sseOptions, servicesAction, args);
        }

        var stdioOptions = new StdioServerOptions
        {
            Name = options.Name,
            Version = options.Version,
            ServerInstructions = options.ServerInstructions
        };
        return StdioServer.RunAsync(stdioOptions, servicesAction, args);
    }
}
