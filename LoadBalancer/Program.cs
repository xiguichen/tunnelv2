// See https://aka.ms/new-console-template for more information

using LoadBalancer;

List<string> WsServerList = new()
{
    "https://raised-invasion-volunteer-offers.trycloudflare.com/TcpHub",
};
Server server = new(WsServerList, 40);
await server.Run(CancellationToken.None);
