using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Diagnostics;
using UnityEngine.Events;
using TMPro;
using LeTai.TrueShadow;
using UnityEngine.EventSystems;
namespace 战雷革命 {
    public static partial class 公共空间 {
        public static void 加载游戏场景(房间数据类 房间数据) {
            待加载房间数据 = 房间数据;
            切换场景("游戏场景");
        }
        public static (string 账号, string 密码) 当前登录;
    }
    public static partial class 本地路径 {
        public static string 主路径 => Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/"));
        public static string 登录缓存路径 => $"{主路径}/缓存/登录信息.json";
    }
    public class 主界面场景控制 : MonoBehaviour {
        public GameObject 模板;
        public GameObject 房间列表;
        public GameObject 连接按钮;
        public TextMeshProUGUI 当前登录玩家;

        public GameObject 登录界面;
        public TMP_InputField 账号输入框;
        public TMP_InputField 密码输入框;
        public TrueShadow 账号输入框阴影;
        public TrueShadow 密码输入框阴影;
        public Button 登录按钮;
        public Button 打开注册界面按钮;
        public TextMeshProUGUI 提示文字;

        public GameObject 注册界面;

        public TMP_InputField 注册界面账号输入框;
        public TMP_InputField 注册界面密码输入框;
        public TrueShadow 注册界面账号输入框阴影;
        public TrueShadow 注册界面密码输入框阴影;
        public TextMeshProUGUI 注册界面提示文字;
        public Button 注册按钮;
        public Button 打开登录界面按钮;

        public Button 切换登录按钮;

        private bool 等待连接;
        private Stopwatch 等待连接超时秒表;
        private 房间数据类 欲连接房间_数据;
        private GameObject 按下的房间按钮;

        public void 显示登录界面() {
            登录界面.SetActive(true);
            if (本地路径.登录缓存路径.文件存在()) {
                var 登录信息 = 本地路径.登录缓存路径.文件读取().JsonToCS<(string 账号, string 密码)>();
                账号输入框.text = 登录信息.账号;
                密码输入框.text = 登录信息.密码;
            }
            登录按钮.onClick.AddListener(() => {
                var 账号 = 账号输入框.text;
                var 密码 = 密码输入框.text;
                if (账号 == "" || 密码 == "") {
                    提示文字.SetText($"{红色str}账号或密码不能为空。");
                    return;
                }
                //账号长度不能大于18
                if (账号.Length > 18) {
                    提示文字.SetText($"{红色str}账号长度不能大于18。");
                    return;
                }
                //密码长度不能小于8或大于18
                if (密码.Length is < 8 or > 18) {
                    提示文字.SetText($"{红色str}密码长度必须在8到18之间。");
                    return;
                }
                提示文字.SetText($"{绿色str}登录中……{默认色str}");
                后台(async _ => {
                    await 信道_房间列表.登录(账号, 密码,
                        成功: () => {
                            主线程(() => {
                                本地路径.登录缓存路径.文件写入((账号, 密码).ToJson());
                                提示文字.SetText($"{绿色str}登录成功{默认色str}");
                                登录成功((账号, 密码));
                            });
                        },
                        失败: 服务器消息 => {
                            主线程(() => {
                                提示文字.SetText($"{红色str}失败{默认色str}：{服务器消息}");
                            });
                        });
                });
            });
            注册按钮.onClick.AddListener(() => {
                var 账号 = 注册界面账号输入框.text;
                var 密码 = 注册界面密码输入框.text;
                if (账号 == "" || 密码 == "") {
                    注册界面提示文字.SetText($"{红色str}账号或密码不能为空。");
                    return;
                }
                //账号长度不能大于18
                if (账号.Length > 18) {
                    注册界面提示文字.SetText($"{红色str}账号长度不能大于18。");
                    return;
                }
                //密码长度不能小于8或大于18
                if (密码.Length is < 8 or > 18) {
                    注册界面提示文字.SetText($"{红色str}密码长度必须在8到18之间。");
                    return;
                }
                后台(async _ => {
                    await 信道_房间列表.注册(账号, 密码,
                        成功: () => {
                            主线程(() => {
                                注册界面提示文字.SetText($"{绿色str}注册成功，请登录。");
                            });
                        },
                        失败: e => {
                            主线程(() => {
                                注册界面提示文字.SetText($"{红色str}失败{默认色str}：{e}");
                            });
                        });
                });

            });
        }
        public void 连接成功() {
            显示登录界面();
            连接按钮文字 = $">>{绿色str}连接成功{默认色str} 点击刷新<<";
            连接按钮.DeleteOnClick();
            连接按钮.OnClick(t => {
                刷新房间列表();
            });
            刷新房间列表();
        }
        public void 连接失败() {
            连接按钮文字 = $">>{红色str}连接失败或已断开{默认色str} 点击重连<<";
            连接按钮.DeleteOnClick();
            连接按钮.OnClick(t => {
                尝试连接();
                t.DeleteOnClick();
            });
            切换登录按钮.gameObject.SetActive(false);
            当前登录玩家.SetText($"{红色str}服务器已断开，请重新登录。");
        }
        public void 登录成功((string 账号, string 密码) 登录数据) {
            当前登录 = 登录数据;
            当前登录玩家.SetText($"{绿色str}当前登录{默认色str}：{当前登录.账号}");
            登录界面.SetActive(false);
            切换登录按钮.gameObject.SetActive(true);
        }
        void Start() {
            //设置默认文本
            账号输入框.placeholder.GetComponent<TextMeshProUGUI>().font = 当前登录玩家.font;
            账号输入框.placeholder.GetComponent<TextMeshProUGUI>().SetText("请输入账号");
            账号输入框.gameObject.OnEnter(_ => {
                账号输入框阴影.enabled = true;
            });
            账号输入框.gameObject.OnExit(_ => {
                账号输入框阴影.enabled = false;
            });
            密码输入框.placeholder.GetComponent<TextMeshProUGUI>().font = 当前登录玩家.font;
            密码输入框.placeholder.GetComponent<TextMeshProUGUI>().SetText("请输入密码");
            密码输入框.gameObject.OnEnter(_ => {
                密码输入框阴影.enabled = true;
            });
            密码输入框.gameObject.OnExit(_ => {
                密码输入框阴影.enabled = false;
            });

            注册界面账号输入框.placeholder.GetComponent<TextMeshProUGUI>().font = 当前登录玩家.font;
            注册界面账号输入框.placeholder.GetComponent<TextMeshProUGUI>().SetText("请输入账号");
            注册界面账号输入框.gameObject.OnEnter(_ => {
                注册界面账号输入框阴影.enabled = true;
            });
            注册界面账号输入框.gameObject.OnExit(_ => {
                注册界面账号输入框阴影.enabled = false;
            });
            注册界面密码输入框.placeholder.GetComponent<TextMeshProUGUI>().font = 当前登录玩家.font;
            注册界面密码输入框.placeholder.GetComponent<TextMeshProUGUI>().SetText("请输入密码");
            注册界面密码输入框.gameObject.OnEnter(_ => {
                注册界面密码输入框阴影.enabled = true;
            });
            注册界面密码输入框.gameObject.OnExit(_ => {
                注册界面密码输入框阴影.enabled = false;
            });

            打开注册界面按钮.onClick.AddListener(() => {
                注册界面.SetActive(true);
            });
            打开登录界面按钮.onClick.AddListener(() => {
                注册界面.SetActive(false);
            });

            切换登录按钮.onClick.AddListener(() => {
                信道_房间列表.断开();
            });

            信道_房间列表.OnConnect += 连接成功;
            信道_房间列表.OnConnecFail += 连接失败;
            信道_房间列表.OnDisconnect += 连接失败;

            连接按钮.OnClick(t => {
                尝试连接();
                t.DeleteOnClick();
            });
            尝试连接();
        }
        bool 等待连接成功() {
            //等十秒，十秒内连接成功，那么跳转场景，否则显示失败。
            Stopwatch 计时器 = Stopwatch.StartNew();
            while (计时器.ElapsedMilliseconds < 10000) {
                信道_游戏.Tick();
                if (信道_游戏.IsConnected) {
                    return true;
                }
            }
            return false;
        }
        //void 检查对局服务器链接状态() {
        //	//等十秒，十秒内连接成功，那么跳转场景，否则显示失败。
        //	if (this.等待连接超时秒表.ElapsedMilliseconds < 10000) {
        //		if (信道_游戏.IsConnected) {
        //			主线程(() => {
        //				加载游戏场景(this.欲连接房间_数据.地图名, this.欲连接房间_数据.每秒同步次数);
        //			});
        //			this.按下的房间按钮.Find("延迟").SetText(绿色str + "连接成功");
        //			this.等待连接 = false;
        //			取消每帧(this.检查对局服务器链接状态);
        //			return;
        //		}
        //	} else {
        //		this.按下的房间按钮.Find("延迟").SetText(红色str + "连接失败");
        //		//移除对秒表的引用以让GC回收对象。
        //		if (this.等待连接超时秒表 != null) {
        //			this.等待连接超时秒表.Stop();
        //			this.等待连接超时秒表 = null;
        //		}
        //		this.等待连接 = false;
        //		取消每帧(this.检查对局服务器链接状态);
        //		return;
        //	}
        //}
        void 测试延迟(string IP, GameObject 延迟文字物体) {
            后台(_ => {
                int 延迟 = (int)信道_房间列表.Ping(IP);
                主线程(() => {
                    if (延迟 == -1) {
                        延迟文字物体.SetText(红色str + "连接失败");
                    } else {
                        延迟文字物体.SetText($"Ping {绿色str}{延迟}ms{默认色str}");
                    }
                });
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
                    foreach (var 房间 in 房间列表数据.房间列表) {
                        添加房间项按钮(房间);
                    }
                });
            });
        }
        //点击按钮后，连接对应服务器，成功后跳转到游戏界面。
        //之后由游戏界面对服务器连接进行进一步处理。
        void 添加房间项按钮(房间数据类 数据) {
            var 按钮 = 模板.Instantiate(房间列表);
            按钮.Find("中间文字").SetActive(false);
            按钮.Find("名称").SetText(数据.房间名);

            string 显示文字 = $"在线{数据.人数} ";
            显示文字 += 数据.房间版本 == 版本 ? $" {绿色str}" : $" {红色str} (本地版本{版本})";
            显示文字 += 数据.房间版本 switch {
                null or "" => "未知版本",
                _ => 数据.房间版本,
            };
            显示文字 += 默认色str;
            显示文字 += $" Tick{数据.每秒同步次数} ";

            var 地图存在 = 地图模板管理器.验证存在性(数据.地图名);
            显示文字 += 地图存在 switch {
                true => $"{绿色str}{数据.地图名}{默认色str}",
                false => $"{红色str}{数据.地图名}{默认色str}(本地不存在)",
            };

            按钮.Find("在线人数").SetText(显示文字);
            测试延迟(数据.IP, 按钮.Find("延迟"));
            if (地图存在) {
                按钮.OnClick(t => {
                    数据.IP.log();
                    //信道_游戏.尝试连接("127.0.0.1", 服务器端口);
                    信道_游戏.尝试连接(数据.IP, 服务器端口);
                    //每帧(信道_游戏.Tick);
                    //尝试连接服务器，成功后跳转到游戏界面。
                    按钮.Find("延迟").SetText(黄色str + "连接中……");
                    后台(_ => {
                        string 文字 = "";
                        if (等待连接成功()) {
                            文字 = 绿色str + "连接成功，加载中……";
                            主线程(() => {
                                公共空间.加载游戏场景(数据);
                            });
                        } else {
                            文字 = 红色str + "连接失败";
                        }
                        //取消每帧(信道_游戏.Tick);
                        主线程(() => {
                            按钮.Find("延迟").SetText(文字);
                        });
                    });
                    t.DeleteOnClick();
                });
            }
        }
    }
}
