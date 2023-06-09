// See https://aka.ms/new-console-template for more information

using LoadBalancer;

List<string> WsServerList = new()
{
    "https://pencil-cargo-auction-america.trycloudflare.com/TcpHub"
};
Server server = new(WsServerList, 10);
await server.Run(CancellationToken.None);
