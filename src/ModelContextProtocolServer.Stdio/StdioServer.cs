﻿using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.Protocol.Types;

namespace ModelContextProtocolServer.Stdio;

public static class StdioServer
{
    public static Task RunAsync(params string[] args)
    {
        var assembly = Assembly.GetEntryAssembly();
        var applicationName = assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? $"mcpserver.{Guid.NewGuid()}.stdio";
        var version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "1.0.0";

        return RunAsync(applicationName, version, args);
    }

    public static Task RunAsync(string applicationName, string version, params string[] args)
    {
        var builder = Host.CreateEmptyApplicationBuilder(settings: new HostApplicationBuilderSettings
        {
            ApplicationName = applicationName,
            Args = args
        });

        builder.Configuration
            .AddCommandLine(args)
            .AddEnvironmentVariables();

        builder.Services
            .AddMcpServer(o => o.ServerInfo = new Implementation
            {
                Name = applicationName,
                Version = version
            })
            .WithStdioServerTransport()
            .WithToolsFromAssembly(Assembly.GetEntryAssembly());

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