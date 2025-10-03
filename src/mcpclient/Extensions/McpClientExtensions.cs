namespace ModelContextProtocol.Client.Extensions;

internal static class McpClientExtensions
{
    public static void DisposeAsyncOnApplicationExit(this McpClient client)
    {
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = false;
            DisposeAsync(client);
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            DisposeAsync(client);
        };
    }

    private static void DisposeAsync(McpClient client)
    {
        client.DisposeAsync().GetAwaiter().GetResult();
        Thread.Sleep(100);
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}