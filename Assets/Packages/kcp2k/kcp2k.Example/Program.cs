//// See https://aka.ms/new-console-template for more information
//using kcp2k;
//using System;
//using System.Linq;
//using System.Threading;

//Console.WriteLine("kcp example");
////设置输出日志信息
//Log.Info = Console.WriteLine;
//Log.Warning = Console.WriteLine;
//Log.Error = Console.WriteLine;

////细节设定
//const ushort port = 7777;
//KcpConfig config = new KcpConfig(
//    NoDelay: true,
//    DualMode: false,
//    Interval: 1,
//    Timeout: 2000,
//    SendWindowSize: Kcp.WND_SND * 1000,
//    ReceiveWindowSize: Kcp.WND_RCV * 1000,
//    CongestionWindow: false,
//    MaxRetransmits: Kcp.DEADLINK * 2
//);
////服务端
//KcpServer server = new KcpServer(
//    OnConnected: (cId) => { },
//    OnData: (cId, message, channel) => Log.Info($"[KCP] OnServerDataReceived({cId}, {BitConverter.ToString(message.Array, message.Offset, message.Count)} @ {channel})"),
//    OnDisconnected: (cId) => { },
//    OnError: (cId, error, reason) => Log.Error($"[KCP] OnServerError({cId}, {error}, {reason}"),
//    config: config
//);

////客户端
//KcpClient client = new KcpClient(
//    OnConnected: () => { },
//    OnData: (消息, 频道) => Log.Info($"[KCP] OnClientDataReceived({BitConverter.ToString(消息.Array, 消息.Offset, 消息.Count)} @ {频道})"),
//    OnDisconnected: () => { },
//    OnError: (error, reason) => Log.Warning($"[KCP] OnClientError({error}, {reason}"),
//    config: config
//);

//// 方便的函数
//void UpdateSeveralTimes(int amount) {
//    // 多次更新以避免测试不稳定。
//    // => 需要更新 120 次以处理默认最大尺寸的消息，
//    //    这需要 120 多个片段。
//    // => 对于默认最大尺寸的 2 倍，需要更频繁地更新。
//    for (int i = 0; i < amount; ++i) {
//        client.Tick(); // 更新客户端状态
//        server.Tick(); // 更新服务器状态
//        // 更新 'interval' 毫秒。
//        // 间隔越低，测试运行得越快。
//        Thread.Sleep((int)config.Interval); // 使当前线程暂停指定的毫秒数
//    }
//}

//// start server
//server.Start(port);

//// connect client
//client.Connect("127.0.0.1", port);
//UpdateSeveralTimes(5);

//// send client to server
//client.Send(new byte[] { 0x01, 0x02 }, KcpChannel.Reliable);
//UpdateSeveralTimes(10);

//// send server to client
//int firstConnectionId = server.connections.Keys.First();
//server.Send(firstConnectionId, new byte[] { 0x03, 0x04 }, KcpChannel.Reliable);
//UpdateSeveralTimes(10);