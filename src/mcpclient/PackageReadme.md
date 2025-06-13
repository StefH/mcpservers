## mcpclient
A MCP client as dotnet tool to invoke tools from a MCP Server (stdio or sse).

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

### Sponsors

[Entity Framework Extensions](https://entityframework-extensions.net/?utm_source=StefH) and [Dapper Plus](https://dapper-plus.net/?utm_source=StefH) are major sponsors and proud to contribute to the development of **mcpclient**.

[![Entity Framework Extensions](https://raw.githubusercontent.com/StefH/resources/main/sponsor/entity-framework-extensions-sponsor.png)](https://entityframework-extensions.net/bulk-insert?utm_source=StefH)

[![Dapper Plus](https://raw.githubusercontent.com/StefH/resources/main/sponsor/dapper-plus-sponsor.png)](https://dapper-plus.net/bulk-insert?utm_source=StefH)