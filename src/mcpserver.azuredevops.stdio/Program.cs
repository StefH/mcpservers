using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocolServer.AzureDevops.Stdio.Services;
using ModelContextProtocolServer.Stdio;

await StdioServer.RunAsync(services =>
{
    services.AddSingleton<AzureDevOpsClient>();
}, args);