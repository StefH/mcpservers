using Microsoft.Extensions.Configuration;
using ModelContextProtocolServer.AzureDevops.Stdio.Services;
using ModelContextProtocolServer.AzureDevops.Stdio.Tools;

var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new List<KeyValuePair<string, string?>>
    {
        new("AZURE_DEVOPS_ORG_URL", "https://dev.azure.com/mstack"),
        new("AZURE_DEVOPS_PAT", Environment.GetEnvironmentVariable("MCP_PAT"))
    })
    .AddEnvironmentVariables()
    .Build();

var azureDevOpsClient = new AzureDevOpsClient(configuration);

var projectsTools = new ProjectsTools(azureDevOpsClient);
var result = await projectsTools.GetProjects(5);

Console.WriteLine(result);