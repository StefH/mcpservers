## ModelContextProtocolServer.Hybrid
Common framework for building a hybrid (Stdio and/or Sse) MCP server.

### Usage
``` csharp
using ModelContextProtocolServer.Hybrid;

await HybridServer.RunAsync(args);
```

If the argument `--stdio` is provided, a Stdio MCP Server is started.
If the argument `--sse` is provided, a Sse MCP Server is started.

### Sponsors

[Entity Framework Extensions](https://entityframework-extensions.net/?utm_source=StefH) and [Dapper Plus](https://dapper-plus.net/?utm_source=StefH) are major sponsors and proud to contribute to the development of **ModelContextProtocolServer.Hybrid**.

[![Entity Framework Extensions](https://raw.githubusercontent.com/StefH/resources/main/sponsor/entity-framework-extensions-sponsor.png)](https://entityframework-extensions.net/bulk-insert?utm_source=StefH)

[![Dapper Plus](https://raw.githubusercontent.com/StefH/resources/main/sponsor/dapper-plus-sponsor.png)](https://dapper-plus.net/bulk-insert?utm_source=StefH)