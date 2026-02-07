using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace ModelContextProtocolServer.Interceptor;

internal class HandlersBuilder(ILogger logger, McpClient client)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    internal McpServerHandlers Build()
    {
        return new McpServerHandlers
        {
            ListToolsHandler = async (_, cancellationToken) => await ListToolsAsync(cancellationToken),
            CallToolHandler = async (request, cancellationToken) => await CallToolAsync(request, cancellationToken)
        };
    }

    private async Task<ListToolsResult> ListToolsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("ListTools - start");
        var tools = await client.ListToolsAsync(cancellationToken: cancellationToken);
        logger.LogInformation("ListTools - end");

        var result = new ListToolsResult
        {
            Tools = tools.Select(t => new Tool
            {
                Name = t.Name,
                Title = t.Title,
                Description = t.Description,
                InputSchema = t.JsonSchema,
                OutputSchema = t.ReturnJsonSchema
            }).ToList()
        };

        logger.LogInformation("ListTools - returning\r\n{Result}", SerializeObject(result));

        return result;
    }

    private async Task<CallToolResult> CallToolAsync(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken)
    {
        if (request.Params?.Name == null)
        {
            throw new McpProtocolException("Missing required parameter 'name'", McpErrorCode.InvalidParams);
        }

        logger.LogInformation("CallTool {Name} - start", request.Params.Name);
        var result = await client.CallToolAsync(request.Params, cancellationToken);
        logger.LogInformation("CallTool {Name} - end", request.Params.Name);

        logger.LogInformation("CallTool - returning\r\n{Result}", SerializeObject(result));

        return result;
    }

    private static string SerializeObject(object obj)
    {
        return JsonSerializer.Serialize(obj, JsonOptions);
    }
}