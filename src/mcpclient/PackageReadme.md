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