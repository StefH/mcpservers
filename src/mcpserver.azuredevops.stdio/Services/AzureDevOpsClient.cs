using Microsoft.Extensions.Configuration;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Stef.Validation;

namespace ModelContextProtocolServer.AzureDevops.Stdio.Services;

internal class AzureDevOpsClient
{
    public ProjectHttpClient ProjectHttpClient { get; }

    public AzureDevOpsClient(IConfiguration configuration)
    {
        var baseUri = Guard.NotNullOrEmpty(configuration["AZURE_DEVOPS_ORG_URL"]);
        var auth = configuration["AZURE_DEVOPS_AUTH_METHOD"] ?? "pat";
        var pat = Guard.Condition(configuration["AZURE_DEVOPS_PAT"], s => !string.IsNullOrEmpty(s) && auth == "pat");

        var connection = new VssConnection(new Uri(baseUri), new VssBasicCredential(string.Empty, pat));

        ProjectHttpClient = connection.GetClient<ProjectHttpClient>();
    }
}