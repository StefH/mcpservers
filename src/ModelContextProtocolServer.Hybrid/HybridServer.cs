using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocolServer.Sse;
using ModelContextProtocolServer.Stdio;

namespace ModelContextProtocolServer.Hybrid;

public static class HybridServer
{
    public static Task RunAsync(params string[] args)
    {
        return RunAsync((services, config) => { }, args);
    }

    public static Task RunAsync(Action<IServiceCollection> servicesAction, params string[] args)
    {
        return RunAsync((services, _) => servicesAction(services), args);
    }

    public static Task RunAsync(Action<IServiceCollection, IConfiguration> action, params string[] args)
    {
        var assembly = Assembly.GetEntryAssembly();
        var applicationName = assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? $"mcpserver.{Guid.NewGuid()}";
        var version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "1.0.0";

        return RunAsync(applicationName, version, action, args);
    }

    public static Task RunAsync(string applicationName, string version, Action<IServiceCollection, IConfiguration> servicesAction, params string[] args)
    {
        if (args.Contains("--sse"))
        {
            return SseServer.RunAsync(applicationName, version, servicesAction, args);
        }

        return StdioServer.RunAsync(applicationName, version, servicesAction, args);
    }
}