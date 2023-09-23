using LoadBalancer;

var ws_server = Environment.GetEnvironmentVariable("WS_SERVER");

var arg = Environment.GetCommandLineArgs();

List<string> wsServerList = new()
{
};


if(arg.Length > 1)
{
    wsServerList.Add(arg[1]);
}
else if (!string.IsNullOrEmpty(ws_server))
{
    wsServerList.Add(ws_server);
}
else
{
    wsServerList.Add("https://apache-italian-powered-massive.trycloudflare.com");
}
Server server = new(wsServerList, 40);
await server.Run(CancellationToken.None);
