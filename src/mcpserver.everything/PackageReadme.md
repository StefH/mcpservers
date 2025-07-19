# mcpserver.everything
A hybrid (Stdio and SSE) MCP server as dotnet tool with some features of the MCP protocol: 
- Echo
- Add
- AddComplex

## `dnx`

``` cmd
dnx 
```

## `dotnet tool`

### Installation
``` cmd
dotnet tool install --global mcpserver.everything
```

### Usage

#### Start as Stdio
``` ps
mcpserver.everything --stdio
```

##### Start as Sse
``` ps
mcpserver.everything --sse
```


## Sponsors

[Entity Framework Extensions](https://entityframework-extensions.net/?utm_source=StefH) and [Dapper Plus](https://dapper-plus.net/?utm_source=StefH) are major sponsors and proud to contribute to the development of **mcpserver.everything**.

[![Entity Framework Extensions](https://raw.githubusercontent.com/StefH/resources/main/sponsor/entity-framework-extensions-sponsor.png)](https://entityframework-extensions.net/bulk-insert?utm_source=StefH)

[![Dapper Plus](https://raw.githubusercontent.com/StefH/resources/main/sponsor/dapper-plus-sponsor.png)](https://dapper-plus.net/bulk-insert?utm_source=StefH)