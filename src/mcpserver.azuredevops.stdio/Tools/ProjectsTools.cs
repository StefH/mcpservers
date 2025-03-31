using System.ComponentModel;
using Microsoft.TeamFoundation.Core.WebApi;
using ModelContextProtocol.Server;
using ModelContextProtocolServer.AzureDevops.Stdio.Services;

namespace ModelContextProtocolServer.AzureDevops.Stdio.Tools;

[McpServerToolType]
internal class ProjectsTools(AzureDevOpsClient azureDevOpsClient) : BaseTools
{
    [McpServerTool, Description("Get Azure DevOps projects.")]
    public async Task<string> GetProjects(
        [Description("Number of team projects to return.")] int? top = null,
        [Description("Number of team projects to skip.")] int? skip = null)
    {
        var allProjects = new List<TeamProjectReference>();

        string? continuationToken = null;
        do
        {
            var projects = await azureDevOpsClient.ProjectHttpClient.GetProjects(top: top, skip: skip, continuationToken: continuationToken);
            allProjects.AddRange(projects);

            skip += projects.Count;

            if (top.HasValue && projects.Count < top)
            {
                continuationToken = projects.ContinuationToken;
            }
        }
        while (continuationToken != null);

        return ToJson(allProjects);
    }
}