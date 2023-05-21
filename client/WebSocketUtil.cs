using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace client
{
    public class WebSocketUtil
    {
        // 创建WebSocket连接
        public static HubConnection CreateHubConnection()
        {
            return new HubConnectionBuilder()
                .WithUrl("http://localhost:5237/TcpHub")
                //.WithUrl("https://7i240740g5.goho.co/TcpHub")
                .Build();
        }

        // 处理创建socket连接
        public static void HandleSocketConnection(HubConnection hubConnection)
        {
            hubConnection.On<string>("Connect", async (connectionId) =>
            {
                Console.WriteLine($"Connectting to {_portId}; ConnectionId: {connectionId}");
                var client = new TcpClient();
                await client.ConnectAsync("127.0.0.1", _portId);
                _tcpClientDict.Add(connectionId, client);
            });
        }

        // 处理发送数据到socket
        public static void HandleWebSocketData(HubConnection hubConnection)
        {
            hubConnection.On<string, byte[], int>("Data", async (connectionId, bytes, length) =>
            {
                var client = _tcpClientDict[connectionId];
                var stream = client.GetStream();
                await stream.WriteAsync(bytes, 0, length);
                _ = HandleSocketData(client, hubConnection, connectionId);
            });
        }

        // 处理从现有的socket中接收数据，并将数据返回给WebSocket连接
        private static async Task HandleSocketData(TcpClient tcpClient, HubConnection hubConnection, string connectionId)
        {
            NetworkStream stream = tcpClient.GetStream();
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesReceived = await stream.ReadAsync(buffer);

                if (bytesReceived == 0)
                {
                    // TODO: notify server that the client disconnected
                    //await hubConnection.SendAsync("Close", connectionId).ConfigureAwait(false);
                    //break;
                    await Task.Delay(100);
                }

                // notify server data received
                Console.WriteLine("Tcp -> WS");
                await hubConnection.SendAsync("Data", connectionId, buffer, bytesReceived).ConfigureAwait(false);
            }
        }

        private static readonly int _portId = 1081;
        private static Dictionary<string, TcpClient> _tcpClientDict = new Dictionary<string, TcpClient>();
    }
}
