using client;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string wsUrl = GetCommandLineArg("--ws-url", args);
        var connection = WebSocketUtil.CreateHubConnection(wsUrl);
        WebSocketUtil.HandleSocketConnection(connection);
        WebSocketUtil.HandleWebSocketData(connection);
        await connection.StartAsync().ConfigureAwait(false);
        while (true)
        {
            await Task.Delay(1000).ConfigureAwait(false);
        }
    }

    static string GetCommandLineArg(string argName, string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == argName && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }

        return null;
    }

}