using ModelContextProtocolServer.Stdio;

namespace ModelContextProtocolServer.Sse;

public class SseServerOptions : BaseServerOptions
{
    protected override string Type => "sse";
}