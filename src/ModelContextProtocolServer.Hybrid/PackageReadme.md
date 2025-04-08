## ModelContextProtocolServer.Hybrid
Common framework for building a hybrid (Stdio and/or Sse) MCP server.

### Usage
``` csharp
using ModelContextProtocolServer.Hybrid;

await HybridServer.RunAsync(args);
```

If the argument `--stdio` is provided, a Stdio MCP Server is started.
If the argument `--sse` is provided, a Sse MCP Server is started.