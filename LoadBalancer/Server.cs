namespace LoadBalancer;

using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.SignalR.Client;

internal class Server
{


    public Server(IList<string> WsServerList, int connectionPerServer)
    {
        this.connectionsPerServer = connectionPerServer;
        this.WsServerList = WsServerList;
        this.hubConnections = new List<HubConnection>();
    }

    public static HubConnection CreateHubConnection(string wsUrl) => new HubConnectionBuilder()
            .WithUrl(wsUrl)
            .Build();


    public void CreateHubConnections()
    {
        foreach (var url in this.WsServerList)
        {
            for (var i = 0; i < this.connectionsPerServer; i++)
            {
                var hubConnection = CreateHubConnection(url);
                _ = hubConnection.On<string, byte[]>("Data", async (connectionId, bytes) =>
                {
                    Console.WriteLine($"WS -> Tcp (Data); ConnectionId: {connectionId}; Length={bytes.Length}");
                    var client = this.GetTcpClientByConnectionId(connectionId);
                    var stream = client.GetStream();
                    await stream.WriteAsync(bytes);
                    Console.WriteLine($"WS -> Tcp (Data); ConnectionId: {connectionId}; Length={bytes.Length}; Data Writed");
                });
                this.hubConnections.Add(hubConnection);
            }
        }
    }

    private async Task StartTcpServer(CancellationToken cancellationToken)
    {
        TcpListener listener = new(IPAddress.Any, Port);
        listener.Start();
        while (!cancellationToken.IsCancellationRequested)
        {
            // accpect the connection
            var client = await listener.AcceptTcpClientAsync(cancellationToken);

            // give a connection id to the client
            var connectionId = Guid.NewGuid().ToString();

            // add the connection id to the map
            this.tcpClientDict.Add(connectionId, client);

            // find a hub connection to handle the connection request
            var connection = this.FindOneHubConnection();

            // send connection request
            await SendConnectRequestAsync(connection, connectionId);

            // if the connection was accepted, we can now start some task for redirect the traffic to remote
            _ = Task.Run(() => Tcp2WebSocket(connection, client, connectionId).ConfigureAwait(false));
        }

    }

    private async Task StartWebSockets(CancellationToken cancellationToken)
    {
        foreach (var hubConnection in this.hubConnections)
        {
            await hubConnection.StartAsync(cancellationToken);
        }
    }

    private static async Task SendConnectRequestAsync(HubConnection hubConnection, string connectionId) => await hubConnection.SendAsync("Connect", connectionId).ConfigureAwait(false);

    private static async Task SendDataRequestAsync(HubConnection hubConnection, string connectionId, byte[] data) => await hubConnection.SendAsync("Data", connectionId, data).ConfigureAwait(false);

    private static async Task SendCloseRequestAsync(HubConnection hubConnection, string connectionId) => await hubConnection.SendAsync("Close", connectionId).ConfigureAwait(false);

    public async Task Run(CancellationToken cancellationToken)
    {
        this.CreateHubConnections();
        await this.StartWebSockets(cancellationToken);
        await this.StartTcpServer(cancellationToken);
    }


    private readonly IList<string> WsServerList;
    private readonly int connectionsPerServer;
    private readonly IList<HubConnection> hubConnections;
    private readonly Dictionary<string, TcpClient> tcpClientDict = new();
    private const int Port = 1080;

    private TcpClient GetTcpClientByConnectionId(string connectionId) => this.tcpClientDict[connectionId];

    private HubConnection FindOneHubConnection()
    {
        Random random = new();
        var i = random.Next(this.WsServerList.Count * this.connectionsPerServer);
        return this.hubConnections[i];
    }

    private static async Task Tcp2WebSocket(HubConnection connection, TcpClient client, string connectionId)
    {
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[1024 * 16];
            while (true)
            {
                var bytesReceived = stream.Read(buffer);
                if (bytesReceived == 0)
                {

                    Console.WriteLine($"Tcp2WebSocket remote socket connection closed. ConnectionId={connectionId}");
                    break;
                }

                var bytes = new byte[bytesReceived];
                for (var i = 0; i < bytesReceived; i++)
                {
                    bytes[i] = buffer[i];
                }

                // notify server data received
                Console.WriteLine($"Tcp -> WS (Data) ; Length: {bytesReceived}");
                await SendDataRequestAsync(connection, connectionId, bytes).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Got exception {e} when receive TCP data");
            Console.WriteLine("Tcp -> WS (Close)");
            await SendCloseRequestAsync(connection, connectionId);
        }
    }

}


