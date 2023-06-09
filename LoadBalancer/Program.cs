// See https://aka.ms/new-console-template for more information

using LoadBalancer;

List<string> WsServerList = new()
{
    "https://billion-branch-basics-dg.trycloudflare.com/TcpHub",
    "https://gm-risk-lf-declaration.trycloudflare.comTcpHub",
    "https://routines-restaurant-df-driver.trycloudflare.comTcpHub",
    "https://bench-accessed-canvas-declare.trycloudflare.comTcpHub",
    "https://bachelor-stages-solely-ad.trycloudflare.comTcpHub"
};
Server server = new(WsServerList, 10);
await server.Run(CancellationToken.None);
