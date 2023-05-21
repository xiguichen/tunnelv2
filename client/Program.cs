using client;

var connection = WebSocketUtil.CreateHubConnection();
WebSocketUtil.HandleSocketConnection(connection);
WebSocketUtil.HandleWebSocketData(connection);
await connection.StartAsync().ConfigureAwait(false);
while(true)
{
    await Task.Delay(1000).ConfigureAwait(false);
}