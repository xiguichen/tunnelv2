// See https://aka.ms/new-console-template for more information

using LoadBalancer;

List<string> WsServerList = new()
{
    "http://localhost:5081/TcpHub"
};
Server server = new(WsServerList, 10);
await server.Run(CancellationToken.None);
