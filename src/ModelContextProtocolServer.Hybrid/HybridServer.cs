using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocolServer.Stdio;

namespace ModelContextProtocolServer.Hybrid;

public static class HybridServer
{
    public static Task RunAsync(params string[] args)
    {
        return RunAsync(_ => { }, args);
    }

    public static Task RunAsync(Action<IServiceCollection> servicesAction, params string[] args)
    {
        var assembly = Assembly.GetEntryAssembly();
        var applicationName = assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? $"mcpserver.{Guid.NewGuid()}";
        var version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "1.0.0";

        return RunAsync(applicationName, version, servicesAction, args);
    }

    public static Task RunAsync(string applicationName, string version, Action<IServiceCollection> servicesAction, params string[] args)
    {
        if (args.Contains("--stdio"))
        {
            return StdioServer.RunAsync(applicationName, version, servicesAction, args);
        }

        if (args.Contains("--sse"))
        {
            return StdioServer.RunAsync(applicationName, version, servicesAction, args);
        }

        throw new ArgumentException("The argument '--stdio' or '--sse' should be provided.");
    }
}