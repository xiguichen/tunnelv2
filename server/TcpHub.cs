using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.Net.Sockets;

namespace server
{
    public class TcpHub: Hub
    {

        public override Task OnConnectedAsync()
        {
            tokenSource = new CancellationTokenSource();
            OpenTcpConnection();
            _ = StartTcpServer(this.Clients.Caller, tokenSource.Token);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            CloseTcpConnection();
            if(tokenSource != null)
            {
                tokenSource.Cancel();
            }
            return base.OnDisconnectedAsync(exception);
        }

        private void OpenTcpConnection()
        {
            Console.WriteLine($"Websocket client connected, ConnectionId id: {this.Context.ConnectionId}");
        }

        private void CloseTcpConnection()
        {
            Console.WriteLine($"Websocket client disconnected, ConnectionId: {this.Context.ConnectionId}");
        }


        public static async Task StartTcpServer(IClientProxy clientProxy,CancellationToken cancellationToken)
        {
            // listen to local port
            TcpListener listener = new(IPAddress.Any, _port);
            listener.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                // accept the connection
                TcpClient client = await listener.AcceptTcpClientAsync();
               
                // give a connection id to the client
                var connectionId = Guid.NewGuid().ToString();
                Console.WriteLine($"Tcp client {connectionId} connected");

                _tcpClients.Add(connectionId, client);

                Console.WriteLine($"WS -> TCP; Connect ; ConnectionId : {connectionId}");
                // notify client to create a connection
                await clientProxy.SendAsync("Connect", connectionId);

                // start a task to read tcp client data and send to websocket
                _ = Tcp2WebSocket(clientProxy, client, connectionId);

            }

        }

        public async Task Data(string connectionId, byte[] bytes, int length)        
        {
            Console.WriteLine($"WS -> TCP (Data): connectionId: {connectionId} ; Length: {length}");
            await _tcpClients[connectionId].GetStream().WriteAsync(bytes,0, length);
        }

        public void Close(string connectionId)
        {
            try
            {
                Console.WriteLine($"WS -> TCP (Close): connectionId: {connectionId};");
                _tcpClients[connectionId].Dispose();
            }
            catch (Exception e)
            {
                throw;
            }
        }


        private static async Task Tcp2WebSocket(IClientProxy clientProxy, TcpClient client, string connectionId)
        {
            NetworkStream stream = client.GetStream();
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesReceived = await stream.ReadAsync(buffer);

                if (bytesReceived == 0)
                {
                    // TODO: notify server that the client disconnected
                    await clientProxy.SendAsync("Close").ConfigureAwait(false);
                    break;
                    //await Task.Delay(100);
                    //continue;
                }

                // notify server data received
                Console.WriteLine($"Tcp -> WS (Data) ; Length: {bytesReceived}");
                await clientProxy.SendAsync("Data", connectionId, buffer, bytesReceived).ConfigureAwait(false);
            }
        }

        private static int _port = 1080;
        private static Dictionary<string, TcpClient> _tcpClients = new Dictionary<string, TcpClient>();
        private static CancellationTokenSource? tokenSource;
    }
}
