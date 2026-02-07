# mcpserver.interceptor
A Stdio MCP server as dotnet tool (or dnx) which can be used as an interceptor for another MCP server.
All tool calls requests and responses are logged to file.

## `dotnet tool`

### Installation
``` cmd
dotnet tool install --global mcpserver.interceptor
```

## Usage

### Start

The next command will start the interceptor which will forward all requests to the `mcpserver.everything.stdio` server.
``` ps
dnx mcpserver.interceptor --yes --serverName=Everything --command=dnx mcpserver.everything.stdio@0.7.0-preview-01 --yes
```

## Sponsors

[Entity Framework Extensions](https://entityframework-extensions.net/?utm_source=StefH) and [Dapper Plus](https://dapper-plus.net/?utm_source=StefH) are major sponsors and proud to contribute to the development of **mcpserver.everything**.

[![Entity Framework Extensions](https://raw.githubusercontent.com/StefH/resources/main/sponsor/entity-framework-extensions-sponsor.png)](https://entityframework-extensions.net/bulk-insert?utm_source=StefH)

[![Dapper Plus](https://raw.githubusercontent.com/StefH/resources/main/sponsor/dapper-plus-sponsor.png)](https://dapper-plus.net/bulk-insert?utm_source=StefH)