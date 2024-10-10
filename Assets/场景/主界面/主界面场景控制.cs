using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Diagnostics;
using UnityEngine.Events;
namespace 战雷革命 {
    public static partial class 公共空间 {
        public static void 加载游戏场景(string 地图名, int Tick) {
            游戏场景地图_待加载 = 地图名;
            Tick_当前加载 = Tick;
            切换场景("游戏场景");
        }
    }
    public class 主界面场景控制 : MonoBehaviour {
        public GameObject 模板;
        public GameObject 房间列表;
        public GameObject 连接按钮;
        public void 连接成功() {
            连接按钮文字 = $">>{绿色str}连接成功{默认色str} 点击刷新<<";
            连接按钮.OnClick(t => {
                刷新房间列表();
                t.DeleteOnClick();
            });
            刷新房间列表();
        }
        public void 连接失败() {
            连接按钮文字 = $">>{红色str}连接失败{默认色str} 点击重连<<";
            连接按钮.OnClick(t => {
                尝试连接();
                t.DeleteOnClick();
            });
        }

        void Start() {
            信道_房间列表.OnConnect += 连接成功;
            信道_房间列表.OnConnecFail += 连接失败;
            信道_房间列表.OnDisconnect += 连接失败;

            连接按钮.OnClick(t => {
                尝试连接();
                t.DeleteOnClick();
            });
        }
        string 连接按钮文字 {
            get {
                return 连接按钮.Find("中间文字").GetText();
            }
            set {
                连接按钮.Find("中间文字")?.SetText(value);
            }
        }
        void 尝试连接() {
            连接按钮文字 = "正在连接...";
            后台(_ => {
                信道_房间列表.连接();
            });
        }
        void 刷新房间列表() {
            后台(async _ => {
                var 房间列表数据 = await 信道_房间列表.获取房间列表();
                主线程(() => {
                    房间列表.Clear(t => t.name != "刷新按钮");
                    房间列表数据.ToJson().Log();
                    foreach (var 房间 in 房间列表数据.房间列表) {
                        添加房间项按钮(房间);
                    }
                });
            });
        }
        void 添加房间项按钮(房间数据类 数据) {
            var 按钮 = 模板.Instantiate(房间列表);
            按钮.Find("中间文字").SetActive(false);
            按钮.Find("名称").SetText(数据.房间名);
            if (数据.房间版本 == 版本) {
                按钮.Find("在线人数").SetText($"在线{数据.人数} 版本 {绿色str}{(数据.房间版本 is null or "" ? "未知" : 数据.房间版本)}{默认色str} Tick{数据.每秒同步次数}");
            } else {
                按钮.Find("在线人数").SetText($"在线{数据.人数} 版本 {红色str}{(数据.房间版本 is null or "" ? "未知" : 数据.房间版本)}{默认色str} (本地版本{版本}) Tick{数据.每秒同步次数}");
            }
            测试延迟(数据.IP, 按钮.Find("延迟"));
            按钮.OnClick(t => {
                数据.IP.log();
                //信道_游戏.尝试连接("127.0.0.1", 服务器端口);
                信道_游戏.尝试连接(数据.IP, 服务器端口);
                每帧(信道_游戏.Tick);
                //尝试连接服务器，成功后跳转到游戏界面。
                按钮.Find("延迟").SetText(黄色str + "连接中……");
                后台(_ => {
                    //等十秒，十秒内连接成功，那么跳转场景，否则显示失败。
                    Stopwatch 计时器 = Stopwatch.StartNew();
                    while (计时器.ElapsedMilliseconds < 10000) {
                        if (信道_游戏.IsConnected) {
                            主线程(() => {
                                加载游戏场景(数据.地图名, 数据.每秒同步次数);
                            });
                            break;
                        }
                    }
                    取消每帧(信道_游戏.Tick);
                    主线程(() => {
                        按钮.Find("延迟").SetText(红色str + "连接超时，请重试。");
                    });
                });
                t.DeleteOnClick();
            });
        }
        void 测试延迟(string IP, GameObject 延迟文字物体) {
            后台(_ => {
                int 延迟 = (int)信道_房间列表.Ping(IP);
                主线程(() => {
                    if (延迟 == -1) {
                        延迟文字物体.SetText(红色str + "连接失败");
                    } else {
                        延迟文字物体.SetText(延迟.ToString());
                    }
                });
            });
        }
    }
}
