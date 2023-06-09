namespace WsServer;
using System.Net.Sockets;
using Microsoft.AspNetCore.SignalR;

public class TcpHub : Hub
{

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"New client connected. Client ConnectionId: {this.Context.ConnectionId}");
        return base.OnConnectedAsync();
    }


    public async Task Connect(string connectionId)
    {
        // create a tcp connection to remote server
        TcpClient client = new();
        Console.WriteLine("Connecting to 127.0.0.1:1081");
        await client.ConnectAsync("127.0.0.1", 1081);
        Console.WriteLine("Connected to 127.0.0.1:1081");
        TcpClients.Add(connectionId, client);
        HandleSocketData(this.Clients.Caller, connectionId, client);
    }

    public async Task Data(string connectionId, byte[] data)
    {
        Console.WriteLine($"WS -> Tcp (Data); ConnectionId={connectionId}; Length={data.Length}");
        var client = GetTcpClientByConnectionid(connectionId);
        if (client != null)
        {
            var stream = client.GetStream();
            await stream.WriteAsync(data).ConfigureAwait(false);
        }
        else
        {
            Console.WriteLine($"WARN: TcpClient is null. Method=Data");
        }
    }

    public void Close(string connectionId)
    {
        Console.WriteLine($"WS -> Tcp (Close); ConnectionId={connectionId}");
        var client = GetTcpClientByConnectionid(connectionId);
        client?.Close();
        _ = TcpClients.Remove(connectionId);
    }


    private static async Task HandleSocketData(ISingleClientProxy caller, string connectionId, TcpClient client)
    {
        try
        {
            var stream = client.GetStream();
            while (true)
            {
                var buffer = new byte[1024 * 16];
                var bytesReceived = await stream.ReadAsync(buffer);

                if (bytesReceived == 0)
                {
                    Console.WriteLine($"Tcp -> WS (Close); ConnectionId={connectionId}");
                    await caller.SendAsync("Close", connectionId).ConfigureAwait(false);
                    break;
                }

                Console.WriteLine($"Tcp -> WS (Data); ConnectionId={connectionId}; Length={bytesReceived}");
                var data = new byte[bytesReceived];
                for (var i = 0; i < bytesReceived; i++)
                {
                    data[i] = buffer[i];
                }
                await caller.SendAsync("Data", connectionId, data).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Got exception during handle socket data: {e}");
            throw;
        }

    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected. ConnectionId={this.Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }


    private static TcpClient? GetTcpClientByConnectionid(string connectionId)
    {
        if (TcpClients.TryGetValue(connectionId, out var client))
        {
            return client;
        }
        return null;
    }

    private static readonly Dictionary<string, TcpClient> TcpClients = new();

}
