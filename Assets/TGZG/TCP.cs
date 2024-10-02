using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;
using static TGZG.公共空间;

namespace TGZG {
    public static partial class 公共空间 {
        public static string ToJson(this object X, bool 完全保存 = false, bool 格式美化 = true) {
            return JsonConvert.SerializeObject(X, new JsonSerializerSettings {
                TypeNameHandling = 完全保存 ? TypeNameHandling.All : TypeNameHandling.None,//不要还原为基类
                //ContractResolver = new IgnoreActionContractResolver(),//不要序列化委托与属性
                MetadataPropertyHandling = 完全保存 ? MetadataPropertyHandling.Default : MetadataPropertyHandling.Ignore,
                Formatting = 格式美化 ? Formatting.Indented : Formatting.None,//美化格式
                PreserveReferencesHandling = 完全保存 ? PreserveReferencesHandling.Objects : PreserveReferencesHandling.None,//保留引用
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,//循环引用
            });
        }
        public static T JsonToCS<T>(this string X, bool 完全保存 = false, bool 格式美化 = true) {
            return JsonConvert.DeserializeObject<T>(X, new JsonSerializerSettings {
                TypeNameHandling = 完全保存 ? TypeNameHandling.All : TypeNameHandling.None,
                //ContractResolver = new IgnoreActionContractResolver(),
                MetadataPropertyHandling = 完全保存 ? MetadataPropertyHandling.Default : MetadataPropertyHandling.Ignore,
                Formatting = 格式美化 ? Formatting.Indented : Formatting.None,//美化格式
                PreserveReferencesHandling = 完全保存 ? PreserveReferencesHandling.Objects : PreserveReferencesHandling.None,//保留引用
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            });
        }
    }
    public class TcpClient {
        public Action OnConnect;
        public Action<ITcpClientBase> OnDisconnect;
        public Action<string> OnSend;
        public Action<string, TouchSocket.Sockets.TcpClient> OnReceive;
        public Dictionary<string, Action<Dictionary<string, string>>> OnRead = new();
        public string IP = "127.0.0.1:7789";
        public TouchSocket.Sockets.TcpClient Client = new();
        public int ID = 0;
        public Dictionary<string, Action<Dictionary<string, string>>> Success = new();
        public TcpClient() {

        }
        public TcpClient(string X) {
            IP = X;
        }
        public TcpClient(string X, string Y) {
            IP = X;
            OnConnect += () => {
                Send(new() { { "标题", "_版本检测" }, { "版本", Y } }, t => {
                    if (t["版本正确"] == "错误") {
                        Client.Close();
                    }
                });
            };
        }
        public void Stop() {
            Client.Close();
        }
        public bool Start() {
            Client.Connected = (client, e) => OnConnect?.Invoke();
            Client.Disconnected = (client, e) => OnDisconnect?.Invoke(client);
            Client.Received = (client, byteBlock, requestInfo) => {
                OnReceive?.Invoke(byteBlock.ToString(), client);
                var A = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len).JsonToCS<Dictionary<string, string>>();
                主线程(() => {
                    if (A.ContainsKey("_ID")) {
                        Success[A["_ID"]](A);
                        Success.RemoveKey(A["_ID"]);
                    } else if (OnRead.ContainsKey(A["标题"])) {
                        OnRead[A["标题"]](A);
                    }
                });
            };
            Client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost(IP))
                .UsePlugin()
                //.ConfigurePlugins(a => a.UseReconnection(5, true, 1000))
                .SetBufferLength(1024 * 64)
                .SetDataHandlingAdapter(() => new FixedHeaderPackageAdapter() { FixedHeaderType = FixedHeaderType.Int }));
            try {
                Client.Connect();
            } catch (Exception e) {
                $"TCP连接失败：{e.Message}".log();
                return false;
            }
            return true;
        }
        public async Task<string> GetPing() {
            var 服务器发送时间 = await SendAsync(new() { { "标题", "_GetPing" } });
            var 当前时间 = DateTime.Now;
            return (当前时间 - 服务器发送时间["_服务器发送时间"].JsonToCS<DateTime>(false)).TotalMilliseconds.ToString() + "ms";
        }
        public async Task<string> Get丢包() {
            return "123";
        }
        public async Task<string> Get传输速度() {
            return "123";
        }
        public void Send(Dictionary<string, string> X, Action<Dictionary<string, string>> Y = null) {
            try {
                X["_ID"] = ID++.ToString();
                if (Y != null) Success[X["_ID"]] = Y;
                var A = X.ToJson(false);
                Client.Send(A);
                OnSend?.Invoke(A);
            } catch {
                UnityEngine.Debug.LogError("TCP发送失败");
                Success.RemoveKey(X["_ID"]);
            }
        }
        public async Task<Dictionary<string, string>> SendAsync(Dictionary<string, string> X) {
            var tcs = new TaskCompletionSource<Dictionary<string, string>>();
            Send(X, result => {
                tcs.SetResult(result);
            });
            return await tcs.Task;
        }
    }
    public class TcpServer {
        public int Port;
        public Dictionary<string, Func<Dictionary<string, string>, SocketClient, Dictionary<string, string>>> OnRead = new();
        public Action<string, SocketClient> OnReceive;
        public Action<SocketClient> OnConnect;
        public Action<SocketClient> OnDisconnect;
        public string Version;
        public TcpService Server = new TcpService();
        public TcpServer(int port) {
            Port = port;
        }
        public TcpServer(int port, string Y) {
            Port = port;
            Version = Y;
        }
        public void Start() {
            Server.Connected = (client, e) => {
                OnConnect?.Invoke(client);
            };
            Server.Disconnected = (client, e) => {
                OnDisconnect?.Invoke(client);
            };
            Server.Received = (client, byteBlock, requestInfo) => {
                OnReceive?.Invoke(byteBlock.ToString(), client);
                var A = byteBlock.ToString().JsonToCS<Dictionary<string, string>>();
                var B = OnRead[A["标题"]](A, client);
                if (B != null) {
                    B["_ID"] = A["_ID"];
                    client.Send(B.ToJson(false));//将收到的信息直接返回给发送方
                }
            };
            OnRead["_版本检测"] = (t, c) => {
                if (t["版本"] == Version) {
                    return new Dictionary<string, string> { { "版本正确", "正确" } };
                } else {
                    return new Dictionary<string, string> { { "版本正确", "错误" } };
                }
            };
            OnRead["测试信息"] = (t, c) => {
                return new() { { "返回", $"您发来的消息是 {t["内容"]}" } };
            };
            Server.Setup(new TouchSocketConfig()
                .SetListenIPHosts(new IPHost[] { new IPHost(Port) })
                .SetDataHandlingAdapter(() => new FixedHeaderPackageAdapter() { FixedHeaderType = FixedHeaderType.Int }))
            .Start();
        }
        public void AllSend(Dictionary<string, string> X) {
            foreach (var i in Server.GetClients()) {
                i.Send(X.ToJson(false));
            }
        }
    }

}
