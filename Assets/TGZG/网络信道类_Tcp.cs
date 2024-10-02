using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using static TGZG.公共空间;
using TGZG;
using kcp2k;
using System.Text;
using System.Linq;
using UnityEngine;
using System.Diagnostics;

namespace TGZG {
    public abstract class 网络信道类_Tcp {
        public TcpClient 游戏端;
        public string 服务端IP;
        public string 版本;
        public bool IsConnected => 游戏端 != null && 游戏端.Client.Online;
        //事件
        /// <summary>
        /// 连接成功时触发
        /// </summary>
        public event Action OnConnect;
        /// <summary>
        /// 连接失败时触发
        /// </summary>
        public event Action OnConnecFail;
        /// <summary>
        /// 断开连接时触发
        /// </summary>
        public event Action OnDisconnect;
        public 网络信道类_Tcp(string IP_Port, string version) {
            服务端IP = IP_Port;
            版本 = version;
        }
        public void 清理事件() {
            OnConnect = null;
            OnConnecFail = null;
            OnDisconnect = null;
        }
        /// <summary>
        /// 尝试连接，成功触发OnConnect事件，失败触发OnConnecFail事件
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void 连接() {
            if (服务端IP == null) {
                主线程(() => {
                    throw new Exception("服务端IP不能为空");
                });
            }
            断开();
            $"正在以版本[{版本}]尝试连接{服务端IP}".Log();
            游戏端 = new TcpClient(服务端IP, 版本);
            游戏端.OnConnect = () => {
                主线程(() => {
                    OnConnect?.Invoke();
                });
            };
            游戏端.OnDisconnect = (e) => {
                主线程(() => {
                    OnDisconnect?.Invoke();
                });
            };
            游戏端.OnReceive = (t, C) => {
                var A = t.JsonToCS<Dictionary<string, string>>();
                if (A.ContainsKey("错误")) {
                    OnMainThread(() => {

                    });
                }
            };
            if (!游戏端.Start()) {
                主线程(() => {
                    OnConnecFail?.Invoke();
                });
            }
        }
        public void 断开() {
            游戏端?.Stop();
            游戏端 = null;
        }
        public double Ping(string IP) {
            System.Net.NetworkInformation.Ping ping = new();
            PingReply reply = ping.Send(IP);
            // 检查回复状态
            if (reply.Status == IPStatus.Success) {
                return reply.RoundtripTime;
            } else {
                ("Ping失败。状态: " + reply.Status).log();
                return -1;
            }
        }
    }
    public abstract class 网络信道类_Kcp {
        public KcpClient 游戏端;
        public string 服务端IP;
        public ushort 端口;
        public string 版本;
        public bool IsConnected => 游戏端 != null && 游戏端.connected;
        public KcpConfig config;
        //事件
        public event Action OnConnected;
        public event Action<ArraySegment<byte>, KcpChannel> OnData;
        public event Action OnDisconnected;
        public event Action<ErrorCode, string> OnError;


        public Dictionary<string, Func<Dictionary<string, string>, (string, string)[]>> OnRead = new();
        public void 清理事件() {
            OnDisconnected = null;
            OnConnected = null;
        }
        public 网络信道类_Kcp(string version) {
            Log.Info = (s) => s.log();
            Log.Warning = (s) => s.logwarring();
            Log.Error = (s) => s.logerror();
            版本 = version;
            config = new KcpConfig(
                NoDelay: true,
                DualMode: false,
                Interval: 1,
                Timeout: 10000,
                SendWindowSize: Kcp.WND_SND * 1000,
                ReceiveWindowSize: Kcp.WND_RCV * 1000,
                CongestionWindow: false,
                MaxRetransmits: Kcp.DEADLINK * 2
            );
            OnRead["数据错误"] = t => {
                Log.Info($"[KCP] 收到错误标题数据：{t.ToJson()}");
                return null;
            };
            OnData += (数据, 频道) => {


                //处理空数据包
                if (数据.Array == null) {
                    $"[KCP] 收到空数据包，频道：{频道}".logerror();
                    //Send(("标题", "数据错误"), ("内容", "数据内容为空"));
                    return;
                }
                //转码数据包
                string 数据json = Encoding.UTF8.GetString(数据.Array, 数据.Offset, 数据.Count);
                var 解析后 = 数据json.JsonToCS<Dictionary<string, string>>();


                //处理无标题数据包
                if (!解析后.ContainsKey("标题")) {
                    $"[KCP] 收到无标题数据包，频道：{频道}".logerror();
                    //Send(("标题", "数据错误"), ("内容", "数据不含标题"));
                    return;
                }
                var 标题 = 解析后["标题"];

                //处理动态消息，匹配成功后数据流向第二方向，否则进入第一方向 
                //第二数据流向：动态消息处理
                if (动态消息处理器.匹配(解析后))
                    return;
                //第一数据流向：固定消息处理

                //处理异常标题数据包
                if (!OnRead.ContainsKey(标题)) {
                    $"[KCP] 收到未知数据标题，频道：{频道}，标题：{标题}".logerror();
                    //Send(("标题", "数据错误"), ("内容", $"未知数据标题，请检查游戏版本。当前服务器版本：{版本}。发来的标题：{标题}"));
                    return;
                }
                //进入数据处理流程并返回对应消息
                var 返回消息 = OnRead[标题].Invoke(解析后);
                if (返回消息 != null) Send(返回消息);
            };
            OnError += (错误码, 错误信息) => {
                UnityEngine.Debug.LogError($"Kcp错误：{错误信息}");
                //主线程(() => {
                //    throw new Exception($"Kcp错误：{错误信息}");
                //});
            };
        }
        public void 尝试连接(string IP, int 端口) {
            断开();
            服务端IP = IP;
            this.端口 = (ushort)端口;

            游戏端 = new KcpClient(
                OnConnected: () => OnConnected?.Invoke(),
                OnData: (消息, 频道) => OnData?.Invoke(消息, 频道),
                OnDisconnected: () => OnDisconnected?.Invoke(),
                OnError: (error, reason) => OnError?.Invoke(error, reason),
                config: config
            );
            IP.log();
            端口.log();
            游戏端.Connect(IP, (ushort)端口);
            //每帧事件 += 游戏端.Tick;
        }
        public void Tick() {
            游戏端?.Tick();
            动态消息处理器.Tick();
        }
        public void 断开() {
            if (游戏端 != null) {
                //每帧事件 -= 游戏端.Tick;
                游戏端.Disconnect();
            }
            游戏端 = null;
        }
        public void Send(Dictionary<string, string> 数据) {
            if (游戏端 == null) return;
            游戏端.Send(数据.ToJson(false).ToBytes(), KcpChannel.Reliable);
        }
        public void Send(params (string, string)[] 数据) {
            if (游戏端 == null) return;
            var 发送数据 = new Dictionary<string, string>();
            foreach (var 数据项 in 数据) {
                发送数据[数据项.Item1] = 数据项.Item2;
            }
            游戏端.Send(发送数据.ToJson(false).ToBytes(), KcpChannel.Reliable);
        }
        public void WaitFor(string 消息标题, int 超时时间, Action<Dictionary<string, string>> 回调) {
            new 动态消息处理器(消息标题, 超时时间, 回调);
        }
        private class 动态消息处理器 {
            public static List<动态消息处理器> 等待列表 = new();
            public static void Tick() {
                等待列表.RemoveAll(t => t.超时);
            }
            public static bool 匹配(Dictionary<string, string> 消息) {
                var 等待消息 = 等待列表.FirstOrDefault(t => t.等待标题 == 消息["标题"]);
                if (等待消息 == null) return false;
                等待消息.回调?.Invoke(消息);
                等待列表.Remove(等待消息);
                return true;
            }

            public string 等待标题;
            public int 超时时间;
            public Action<Dictionary<string, string>> 回调;
            public bool 超时 => 计时器.ElapsedMilliseconds > 超时时间 * 1000;

            private Stopwatch 计时器;
            public 动态消息处理器(string 等待标题, int 超时时间, Action<Dictionary<string, string>> 回调) {
                this.等待标题 = 等待标题;
                this.超时时间 = 超时时间;
                this.回调 = 回调;
                计时器 = new Stopwatch();
                计时器.Start();
                等待列表.Add(this);
            }
        }
    }
}