# MCP Servers
A collection of MCP (Model Context Protocol) servers as dotnet tools


## 💻 dotnet tool `servers`
| Name | NuGet | Info
| :- | :- | :- 
| `mcpserver.everything.stdio` | [![NuGet Badge](https://img.shields.io/nuget/v/mcpserver.everything.stdio)](https://www.nuget.org/packages/mcpserver.everything.stdio)
| `mcpserver.openxml.sse` | [![NuGet Badge](https://img.shields.io/nuget/v/mcpserver.openxml.sse)](https://www.nuget.org/packages/mcpserver.openxml.sse)
| `mcpserver.openxml.stdio` | [![NuGet Badge](https://img.shields.io/nuget/v/mcpserver.openxml.stdio)](https://www.nuget.org/packages/mcpserver.openxml.stdio)

<hr>

## 📱 dotnet tool `client`
A MCP client as dotnet tool to invoke tools from a MCP Server (stdio or sse).

[![NuGet Badge](https://img.shields.io/nuget/v/mcpclient)](https://www.nuget.org/packages/mcpclient)

### Installation
``` cmd
dotnet tool install --global mcpclient
```

### Usage
``` cmd
mcpclient --command=mcpserver.everything.stdio
```

#### Example
``` raw
Select a tool to execute:

> Add (Adds two numbers)
  Echo (Echoes back the input)
  Quit

Enter value for First number 'a' : 1
Enter value for Second number 'b' : 2

Result: The sum of 1 and 2 is 3.
```


<hr>

## 📦 Projects
| Name | NuGet | Info
| :- | :- | :- 
| `ModelContextProtocolServer.Sse` | [![NuGet Badge](https://img.shields.io/nuget/v/ModelContextProtocolServer.Sse)](https://www.nuget.org/packages/ModelContextProtocolServer.Sse) | Common framework for building a Sse MCP server.
| `ModelContextProtocolServer.Stdio` | [![NuGet Badge](https://img.shields.io/nuget/v/ModelContextProtocolServer.Stdio)](https://www.nuget.org/packages/ModelContextProtocolServer.Stdio) | Common framework for building a Stdio MCP server.