using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;
using System.Linq;
using UnityEngine.UI;
namespace 战雷革命 {
    public static partial class 公共空间 {
        public static string 游戏场景地图_待加载;
        public static int Tick_当前加载;
        public static 玩家登录数据 玩家数据 = new() { tp = default };
        public static GameObject 加载其他玩家飞行器(玩家游玩数据 数据) {
            var 飞行器 = UnityEngine.Object.Instantiate(加载载具(数据.u.tp));
            飞行器.GetComponent<摄像机跟随1>().enabled = false;
            飞行器.GetComponent<翼面控制>().enabled = false;
            var 同步器 = 飞行器.AddComponent<飞行数据同步>();
            同步器.初始化名称UI();
            同步器.登录数据 = 数据.u;
            同步器.世界数据 = 数据.p;
            var 毁伤计算 = 飞行器.GetComponent<毁伤计算>();
            毁伤计算.自动损坏 = false;
            毁伤计算.被击中 += (攻击者, 部位, 伤害值, 损坏) => {
                //当攻击者为主视角玩家时，将伤害值发送至服务器
                if (攻击者 != 玩家数据.n) return;
                var A = new 击伤信息();
                A.ths = 飞行器.name;
                A.bp = 部位;
                A.dm = 伤害值;
                信道_游戏.击伤(A);
            };
            return 飞行器;
        }
        public static GameObject 加载玩家飞行器(玩家登录数据 玩家) {
            var 飞行器 = UnityEngine.Object.Instantiate(加载载具(玩家.tp));
            var 同步器 = 飞行器.AddComponent<飞行数据同步>();
            同步器.初始化名称UI();
            同步器.登录数据 = 玩家;
            var 毁伤计算 = 飞行器.GetComponent<毁伤计算>();
            毁伤计算.被击中 += (攻击者, 部位, 伤害值, 损坏) => {
                if (攻击者 != "") return;//忽略非碰撞伤害
                //上传损坏
                if (损坏) {
                    信道_游戏.损坏(部位);
                    if (部位 == 部位.身) 飞行器.Destroy();
                }
            };
            return 飞行器;
        }
        public static GameObject 加载载具(载具类型 类型) {
            类型.ToString().log();
            if (类型 == 载具类型.f86f25) {
                return 加载资源<GameObject>("Assets/飞行器/F86-F25/F86-F25.prefab");
            } else if (类型 == 载具类型.m15n23) {
                return 加载资源<GameObject>("Assets/飞行器/Mig15-ns23/Mig15-ns23.prefab");
            } else if (类型 == 载具类型.f4c) {
                return 加载资源<GameObject>("Assets/飞行器/F4C/F4C.prefab");
            } else if (类型 == 载具类型.m21pfm) {
                return 加载资源<GameObject>("Assets/飞行器/Mig-21PFM/Mig-21PFM.prefab");
            } else if (类型 == 载具类型.P51h) {
                return 加载资源<GameObject>("Assets/飞行器/P51h/P51h.prefab");
            } else {
                throw new NotImplementedException("载具加载错误：未知载具类型");
            }
        }
        public static GameObject 加载导弹(导弹类型 类型) {
            return UnityEngine.Object.Instantiate(加载资源<GameObject>(导弹预设体[类型]));
        }
    }
    //当成功连接至游戏服务器时，此场景加载，此脚本加载。
    //此脚本负责管理游戏场景的主要逻辑。
    public partial class 游戏场景主程序 : MonoBehaviour {
        private 玩家世界数据 重生点位置 => new() {
            p = new Vector3(UnityEngine.Random.Range(-500, 500), 2.5f, UnityEngine.Random.Range(-500, 500)).To向量3(),
            d = Quaternion.identity.To向量4(),
            v = Vector3.zero.To向量3(),
            r = Vector3.zero.To向量3()
        };
        private Stopwatch 计时器 = Stopwatch.StartNew();
        private bool 游戏开始 = false;
        private bool 准备界面显示 = false;
        private GameObject 玩家飞行器;
        private List<GameObject> 其他玩家飞行器 = new();
        private List<玩家登录数据> 在线玩家 = new();
        [Header("悬浮显示")]
        public GameObject 服务器断链提示;
        public GameObject 消息框;
        public GameObject 消息滚动区;
        public GameObject 清空按钮;
        public GameObject 聊天输入框;
        public GameObject 发送按钮;
        public GameObject 当前在线玩家数量;
        public 飞行数据显示器 飞行数据显示;
        public GameObject 导弹外环;
        public GameObject 导弹内环;
        [Header("准备界面")]
        public GameObject 准备界面背景;
        public GameObject 返回主菜单按钮;
        public GameObject 测试通信按钮;
        public GameObject 重连按钮;
        public GameObject 输入昵称;
        public GameObject 选择载具mig15;
        public GameObject 选择载具F86;
        public GameObject 选择载具F4c;
        public GameObject 选择载具mig21pfm;
        public GameObject 选择载具P51H;

        public GameObject 您已选择;
        public GameObject 登录按钮;
        public GameObject 当前在线玩家列表;
        public GameObject 重生按钮;
        public GameObject 游戏界面背景;
        [Header("死亡界面")]
        public GameObject 死亡界面背景;
        [HideInInspector]
        public 导弹 当前选定导弹;

        List<(int, GameObject)> 管理导弹 = new();
        Dictionary<玩家登录数据, List<(int, GameObject)>> 其他玩家导弹 = new();
        void Start() {
            if (游戏场景地图_待加载 is null or "") throw new Exception("待加载的场景为空");
            加载地图(游戏场景地图_待加载);
            注册UI();
            注册通信();
        }
        void 注册UI() {
            返回主菜单按钮.OnClick(t => {
                信道_游戏.清理事件();
                信道_房间列表.清理事件();
                信道_游戏.断开();
                切换场景("主界面");
            });
            测试通信按钮.OnClick(t => {
                //信道_游戏.SendMsg("1234");
            });
            重连按钮.OnClick(t => {
                信道_游戏.尝试连接("127.0.0.1", 16314);
                服务器断链提示.SetActive(false);
            });
            输入昵称.OnTextChange(t => {
                玩家数据.n = t;
            });
            选择载具mig15.OnClick(t => {
                玩家数据.tp = 载具类型.m15n23;
                您已选择.SetText(绿色str + "您已选择：" + 玩家数据.tp);
            });
            选择载具F86.OnClick(t => {
                玩家数据.tp = 载具类型.f86f25;
                您已选择.SetText(绿色str + "您已选择：" + 玩家数据.tp);
            });
            选择载具F4c.OnClick(t => {
                玩家数据.tp = 载具类型.f4c;
                您已选择.SetText(绿色str + "您已选择：" + 玩家数据.tp);
            });
            选择载具mig21pfm.OnClick(t => {
                玩家数据.tp = 载具类型.m21pfm;
                您已选择.SetText(绿色str + "您已选择：" + 玩家数据.tp);
            });
            选择载具P51H.OnClick(t => {
                玩家数据.tp = 载具类型.P51h;
                您已选择.SetText(绿色str + "您已选择：" + 玩家数据.tp);
            });
            登录按钮.OnClick(t => {
                if (玩家数据.n is null or "") {
                    您已选择.SetText(红色str + "昵称不能为空");
                    return;
                }
                if (玩家数据.n.Length > 15) {
                    您已选择.SetText(红色str + "昵称超长");
                    return;
                }
                if (玩家数据.tp == default) {
                    您已选择.SetText(红色str + "请选择载具");
                    return;
                }
                if (!信道_游戏.IsConnected) {
                    您已选择.SetText(红色str + "服务器未连接");
                    return;
                }
                玩家数据.ToJson().log();
                信道_游戏.登录(玩家数据);
            });
            清空按钮.OnClick(t => {
                消息框.SetText("");
            });
            发送按钮.OnClick(t => {
                var 内容 = 聊天输入框.GetText();
                if (内容 is null or "") return;
                聊天输入框.SetText("");
                信道_游戏.发送聊天消息(玩家数据.n, 内容);
            });
            重生按钮.OnClick(t => {
                玩家飞行器.Destroy();
                玩家飞行器 = 加载玩家飞行器(玩家数据);
                玩家飞行器.GetComponent<飞行数据同步>().世界数据 = 重生点位置;

                死亡界面背景.SetActive(false);
                游戏界面();

                信道_游戏.发送重生(玩家数据);
            });
        }
        void 注册通信() {
            信道_游戏.OnDisconnected += () => {
                服务器断链提示.SetActive(true);
                玩家飞行器.Destroy();
                其他玩家飞行器.ForEach(t => t.Destroy());
            };
            信道_游戏.OnRead["聊天消息"] = t => {
                新增聊天消息(t["发送者"], t["内容"], Enum.Parse<队伍>(t["队伍"]));
                return null;
            };
            信道_游戏.OnRead["登录成功"] = t => {
                t["其他玩家数据"].log();
                var 其他玩家数据 = t["其他玩家数据"].JsonToCS<List<(玩家游玩数据 世界, HashSet<部位> 损坏部位)>>(格式美化: false);
                玩家飞行器 = 加载玩家飞行器(玩家数据);
                玩家飞行器.GetComponent<飞行数据同步>().世界数据 = 重生点位置;
                在线玩家.Add(玩家数据);
                foreach (var 此玩家 in 其他玩家数据) {
                    其他玩家飞行器.Add(加载其他玩家飞行器(此玩家.世界));
                    在线玩家.Add(此玩家.世界.u);
                }
                游戏界面(); 鼠标隐藏(); 准备界面显示 = false;
                游戏开始 = true;
                return null;
            };
            信道_游戏.OnRead["同步其他玩家数据"] = t => {
                var 所有其他玩家 = t["其他玩家"].JsonToCS<List<玩家游玩数据>>();
                //数据同步逻辑
                主线程(() => {
                    更新其他玩家(所有其他玩家);
                    初始化玩家列表();
                });
                return null;
            };
            信道_游戏.OnRead["同步重生"] = t => {
                var 同步玩家 = t["玩家"].JsonToCS<玩家登录数据>();
                //删除本地飞行器，自动重新加载心跳包中的飞行器
                主线程(() => {
                    var 飞行器 = 其他玩家飞行器.FirstOrDefault(t => t.name == 同步玩家.n);
                    其他玩家飞行器.Remove(飞行器);
                    飞行器.Destroy();
                });
                return null;
            };
            信道_游戏.OnRead["导弹发射"] = t => {
                //创建导弹并设置到对应位置
                var 所属玩家 = t["玩家"].JsonToCS<玩家登录数据>();
                var 导弹数据 = t["导弹数据"].JsonToCS<导弹飞行数据>();
                var 导弹对象 = 加载导弹(导弹数据.tp);

                导弹对象.transform.position = 导弹数据.p.ToVector3();
                导弹对象.transform.rotation = 导弹数据.d.ToQuaternion();
                导弹对象.GetComponent<Rigidbody>().velocity = 导弹数据.v.ToVector3();
                导弹对象.GetComponent<Rigidbody>().angularVelocity = 导弹数据.r.ToVector3();
                导弹对象.GetComponent<导弹>().OnStart += () => {
                    导弹对象.GetComponent<导弹>().导引头.GetComponent<近炸控制器>().启动近炸 = false;
                    导弹对象.GetComponent<导弹>().发射();
                };
                if (!其他玩家导弹.ContainsKey(所属玩家)) {
                    其他玩家导弹[所属玩家] = new();
                }
                其他玩家导弹[所属玩家].Add((导弹数据.i, 导弹对象));
                return null;
            };
            信道_游戏.OnRead["导弹爆炸"] = t => {
                var 所属玩家 = t["玩家"].JsonToCS<玩家登录数据>();
                var 导弹数据 = t["导弹数据"].JsonToCS<导弹飞行数据>();
                //找到对应的导弹，爆炸并销毁
                主线程(() => {
                    var 导弹 = 其他玩家导弹[所属玩家].FirstOrDefault(t => t.Item1 == 导弹数据.i);
                    if (导弹 == default) return;//多余信息忽略
                    导弹.Item2.GetComponent<导弹>().爆炸();
                    其他玩家导弹[所属玩家].Remove(导弹);
                });
                return null;
            };
            信道_游戏.OnRead["同步损坏"] = t => {
                var 所属玩家 = t["玩家"];
                var 损坏数据 = t["数据"].JsonToCS<HashSet<部位>>();
                //更新飞行器损坏情况
                主线程(() => {
                    var 飞行器 = 其他玩家飞行器.FirstOrDefault(t => t.name == 所属玩家);
                    if (飞行器 == default) return;
                    飞行器.GetComponent<毁伤计算>().损坏(损坏数据);
                });
                return null;
            };
            信道_游戏.OnRead["被击伤"] = t => {
                var 击伤数据 = t["数据"].JsonToCS<击伤信息>();
                主线程(() => {
                    玩家飞行器.GetComponent<毁伤计算>().伤害(击伤数据.bp, 击伤数据.dm);
                });
                return null;
            };
            信道_游戏.OnRead["死亡"] = t => {
                主线程(() => {
                    死亡界面();
                });
                return null;
            };
            信道_游戏.OnRead["房间内玩家死亡消息"] = t => {
                var 死亡玩家 = t["玩家"];
                主线程(() => {
                    新增聊天消息("系统", $"{红色str}{死亡玩家}{默认色str} 已死亡", 队伍.系统);
                });
                return null;
            };
        }
        List<Stopwatch> 武器开枪计时器 = new();
        void Update() {
            if (游戏开始 && Input.GetKeyDown(KeyCode.Escape)) {
                if (准备界面显示) {
                    游戏界面();
                } else {
                    准备界面();
                }
            }
            //玩家飞行器为空的情况：未登录，未连接服务器，服务器断开
            if (玩家飞行器 != null) {
                //更新参数显示
                var 飞行数据 = 玩家飞行器.GetComponent<飞行数据同步>().世界数据;
                飞行数据显示.速度 = 飞行数据.v.ToVector3().magnitude * 3.6f;
                飞行数据显示.马赫 = 飞行数据.v.ToVector3().magnitude / 340;
                飞行数据显示.高度 = 飞行数据.p[1];
                var 指向角度 = 飞行数据.d.ToQuaternion() * Vector3.forward;
                var 速度方向 = 飞行数据.v.ToVector3().normalized;
                飞行数据显示.攻角 = Vector3.Angle(速度方向, 指向角度);
                飞行数据显示.节流阀 = 玩家飞行器.GetComponent<翼面控制>().节流阀 * 100;
                飞行数据显示.襟翼 = 玩家飞行器.GetComponent<翼面控制>().襟翼 / 玩家飞行器.GetComponent<翼面控制>().襟翼开启值 * 100;
                飞行数据显示.刹车 = 玩家飞行器.GetComponent<翼面控制>().刹车 == 100f;
                //如果玩家按下左键，那么发射子弹
                List<bool> 本帧开枪 = new() { };
                if (Input.GetAxis("Fire1") > 0) {
                    //获取玩家飞行器下所有包含tag：机炮 的GameObject
                    var 机炮 = 玩家飞行器.FindAll(t => t.tag == "机炮");
                    //遍历机炮，发射子弹
                    int num = 0;
                    foreach (var i in 机炮) {
                        var 指示器 = i.GetComponent<枪射指示器>();
                        if (武器开枪计时器.Count() < num + 1) {
                            武器开枪计时器.Add(Stopwatch.StartNew());
                        }
                        if (武器开枪计时器[num].ElapsedMilliseconds > (60 * 1000) / 指示器.每分射速) {
                            本帧开枪.Add(true);
                            指示器.射击(玩家飞行器.transform);
                            武器开枪计时器[num].Restart();
                        }
                        num++;
                    }
                }
                if (Input.GetAxis("Fire1") == 0) {
                    本帧开枪.Clear();
                }
                if (Input.GetKeyDown(KeyCode.LeftAlt)) {
                    var 挂点 = 玩家飞行器.GetComponent<摄像机跟随1>().挂点;
                    foreach (var i in 挂点) {
                        if (i.挂载物体 == null) continue;
                        var 导弹 = i.挂载物体.GetComponent<导弹>();
                        if (i.挂载物体 != null && 导弹 != null) {
                            if (!导弹.已启动) {
                                导弹.启动();
                                渲染导弹锁定圈(导弹);
                            } else {
                                导弹.发射();
                                i.放开挂载();
                                隐藏导弹锁定圈();
                                注册导弹发射(导弹);
                            }
                            break;
                        }
                    }
                }
                if (Input.GetKeyUp(KeyCode.LeftControl) || (当前选定导弹 != null && !当前选定导弹.已启动)) {
                    var 挂点 = 玩家飞行器.GetComponent<摄像机跟随1>().挂点;
                    foreach (var i in 挂点) {
                        if (i.挂载物体 != null && i.挂载物体.tag == "导弹") {
                            var 导弹 = i.挂载物体.GetComponent<导弹>();
                            if (导弹.已启动) {
                                导弹.停止();
                                隐藏导弹锁定圈();
                            }
                        }
                    }
                }
                //时刻更新导弹锁定圈
                if (当前选定导弹 != null) {
                    导弹外环.SetActive(true);
                    导弹内环.SetActive(true);
                    var 外环半径角 = 当前选定导弹.导引头.离轴角度;
                    var 内环半径角 = 当前选定导弹.导引头.视场;
                    var 内环角度 = 当前选定导弹.导引头.导引头转动角度;

                    var 外环宽高 = 获取环宽高(外环半径角);
                    var 内环宽高 = 获取环宽高(内环半径角);
                    导弹外环.GetComponent<RectTransform>().sizeDelta = 外环宽高;
                    导弹内环.GetComponent<RectTransform>().sizeDelta = 内环宽高;
                    var 外环位置 = 获取环位置(new(0, 0));
                    var 内环位置 = 获取环位置(内环角度);
                    导弹内环.GetComponent<RectTransform>().position = new Vector2(内环位置.x, 内环位置.y);
                    导弹外环.GetComponent<RectTransform>().position = new Vector2(外环位置.x, 外环位置.y);

                    if (当前选定导弹.导引头.导引头锁定) {
                        导弹内环.GetComponent<Image>().material.SetColor("_CircleColor", Color.red);
                    } else {
                        导弹内环.GetComponent<Image>().material.SetColor("_CircleColor", Color.white);
                    }
                } else {
                    导弹外环.SetActive(false);
                    导弹内环.SetActive(false);
                }
                //发送数据
                if (计时器.ElapsedMilliseconds > 1000 / Tick_当前加载) {
                    if (游戏开始) {
                        var 玩家同步器 = 玩家飞行器.GetComponent<飞行数据同步>();
                        var 毁伤情况 = 玩家飞行器.GetComponent<毁伤计算>();
                        //管理玩家发射的导弹
                        List<导弹飞行数据> 玩家发射的导弹 = new();
                        foreach (var i in 管理导弹) {
                            var 导弹数据 = new 导弹飞行数据();
                            导弹数据.i = i.Item1;
                            导弹数据.tp = i.Item2.GetComponent<导弹>().类型;
                            导弹数据.p = i.Item2.transform.position.To向量3();
                            导弹数据.d = i.Item2.transform.rotation.To向量4();
                            导弹数据.v = i.Item2.GetComponent<Rigidbody>().velocity.To向量3();
                            导弹数据.r = i.Item2.GetComponent<Rigidbody>().angularVelocity.To向量3();
                            玩家发射的导弹.Add(导弹数据);
                        }
                        var 数据 = new 玩家游玩数据() {
                            u = 玩家同步器.登录数据,
                            p = 玩家同步器.世界数据,
                            射 = 本帧开枪.Select(t => t ? 1 : 0).ToArray(),
                            msl = 玩家发射的导弹
                        };
                        信道_游戏.发送数据(数据);
                    }
                    计时器.Restart();
                }
            }
            信道_游戏.Tick();
        }
        Vector2 获取环宽高(float 半径角) {
            //获取导弹指向万米外的位置
            var 指向位置 = new Vector3(0, 0, 10000);
            var 指向世界坐标 = 当前选定导弹.transform.TransformPoint(指向位置);
            //获取环偏指向万米外的位置;
            var 转向 = Quaternion.Euler(0, 半径角, 0);
            var 转向指向位置 = 转向 * 指向位置;
            var 转向指向世界坐标 = 当前选定导弹.transform.TransformPoint(转向指向位置);
            //获取两点屏幕坐标
            var 屏幕坐标1 = Camera.main.WorldToScreenPoint(指向世界坐标);
            var 屏幕坐标2 = Camera.main.WorldToScreenPoint(转向指向世界坐标);
            //计算两点在屏幕上的距离
            var 距离 = Vector2.Distance(屏幕坐标1, 屏幕坐标2);
            //计算环宽高
            var 宽高 = new Vector2(距离 * 2, 距离 * 2);
            return 宽高;
        }
        Vector2 获取环位置(Vector2 双轴角度) {
            //在玩家本地坐标系下，将前向向量向右转动内环角度.x，然后向上转动内环角度.y
            var 前向向量 = new Vector3(0, 0, 10000);
            var 内环转向 =
                Quaternion.AngleAxis(双轴角度.x, Vector3.up)
                * Quaternion.AngleAxis(双轴角度.y, Vector3.left)
                * 前向向量;
            var 内环转向位置 = 当前选定导弹.transform.TransformPoint(内环转向);
            //获得此位置在玩家摄像机上的屏幕坐标
            return Camera.main.WorldToScreenPoint(内环转向位置);
        }
        public void 准备界面() {
            准备界面背景.SetActive(true);
            游戏界面背景.SetActive(false);
            鼠标显示();
            准备界面显示 = true;
            if (玩家飞行器 != null) 玩家飞行器.GetComponent<摄像机跟随1>().暂停视角控制 = true;
        }
        public void 游戏界面() {
            准备界面背景.SetActive(false);
            游戏界面背景.SetActive(true);
            鼠标隐藏();
            准备界面显示 = false;
            if (玩家飞行器 != null) 玩家飞行器.GetComponent<摄像机跟随1>().暂停视角控制 = false;
        }
        public GameObject 加载地图(string 地图名) {
            var 地图 = 加载资源<GameObject>($"Assets/地图/{地图名}.prefab");
            Instantiate(地图);
            return 地图;
        }
        void 注册导弹发射(导弹 导弹物体) {
            //本地储存对应编号的导弹
            int 当前编号 = 0;
            if (管理导弹.Count != 0) {
                当前编号 = 管理导弹.Max(t => t.Item1) + 1;
            }
            var 此数据 = (当前编号, 导弹物体.gameObject);
            管理导弹.Add(此数据);
            导弹物体.导引头.爆炸控制器.爆炸前 += () => {
                "导弹爆炸".Log();
                信道_游戏.导弹爆炸(获取导弹数据信息(当前编号, 导弹物体.gameObject));
                var 导弹 = 管理导弹.FirstOrDefault(t => t.Item1 == 当前编号);
                if (导弹 != default) 管理导弹.Remove(导弹);
            };
            //提交本地此编号导弹发射的数据
            信道_游戏.导弹发射(获取导弹数据信息(当前编号, 导弹物体.gameObject));
        }
        void 更新其他玩家(List<玩家游玩数据> 数据) {
            //屎山，刷新和初始化在代码上共用一段类似的代码。
            更新或添加其他玩家(数据.Select(t => (t, new HashSet<部位>())).ToList(), false);
        }
        void 更新或添加其他玩家(List<(玩家游玩数据 世界, HashSet<部位> 损坏部位)> 所有其他玩家, bool 初始化 = true) {
            lock (其他玩家飞行器) {
                Action 列表修改事件 = () => { };
                //如果本地没有此玩家，但服务器有，则创建其飞行器
                在线玩家 = 所有其他玩家.Select(t => t.世界.u).ToList();
                在线玩家.Add(玩家数据);
                //如果本地有此玩家，但服务器没有，则销毁其飞行器
                foreach (var 此飞行器 in 其他玩家飞行器) {
                    if (!所有其他玩家.Exists(t => t.世界.u.n == 此飞行器.name)) {
                        var 缓存 = 此飞行器.GetComponent<飞行数据同步>().登录数据;
                        Destroy(此飞行器);
                        列表修改事件 += () => {
                            其他玩家飞行器.Remove(此飞行器);
                        };
                    }
                }
                foreach (var 此玩家 in 所有其他玩家) {
                    var 此 = 其他玩家飞行器.Find(t => t.name == 此玩家.世界.u.n);
                    //如果本地没有此玩家，则创建其飞行器
                    if (此 == null) {
                        此 = 加载其他玩家飞行器(此玩家.世界);
                        var 缓存 = 此玩家.世界.u;
                        列表修改事件 += () => {
                            其他玩家飞行器.Add(此);
                        };
                    }
                    //位置同步
                    此.GetComponent<飞行数据同步>().世界数据 = 此玩家.世界.p;
                    //初始化时断裂同步。一般的断裂同步由服务器更新
                    if (初始化) {
                        此.GetComponent<毁伤计算>().损坏(此玩家.损坏部位);
                    }
                    //射击同步
                    var 机炮 = 此.FindAll(t => t.tag == "机炮");
                    int i = 0;
                    foreach (var j in 机炮) {
                        if (此玩家.世界.射 != null && 此玩家.世界.射.Length > i && 此玩家.世界.射[i] == 1) {
                            j.GetComponent<枪射指示器>().射击(此.transform);
                        }
                        i++;
                    }
                    //导弹同步
                    if (!其他玩家导弹.ContainsKey(此玩家.世界.u)) 其他玩家导弹[此玩家.世界.u] = new();
                    if (此玩家.世界.msl != null) {
                        //对所有导弹数据，找到对应的对象，更新数据位置
                        foreach (var 导弹数据 in 此玩家.世界.msl) {
                            var 导弹对象 = 其他玩家导弹[此玩家.世界.u].FirstOrDefault(t => t.Item1 == 导弹数据.i);
                            if (导弹对象 == default) continue;//多余信息忽略
                            导弹对象.Item2.transform.position = 导弹数据.p.ToVector3();
                            导弹对象.Item2.transform.rotation = 导弹数据.d.ToQuaternion();
                            导弹对象.Item2.GetComponent<Rigidbody>().velocity = 导弹数据.v.ToVector3();
                            导弹对象.Item2.GetComponent<Rigidbody>().angularVelocity = 导弹数据.r.ToVector3();
                        }
                    }
                }
                列表修改事件.Invoke();
            }
        }
        void 初始化玩家列表() {
            当前在线玩家数量.SetText($"当前在线：{在线玩家.Count}");
            当前在线玩家列表.SetText(string.Join(Environment.NewLine, 在线玩家.Select(t => $"{t.n} ({t.tp})")));
        }
        void 新增聊天消息(string 发送者, string 内容, 队伍 队伍) {
            var 消息 = DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine;
            if (队伍 == 队伍.无) {
                消息 += $"[{灰色str}无{默认色str}] {发送者} ：";
            } else if (队伍 == 队伍.红) {
                消息 += $"[{红色str}红方{默认色str}] {发送者} ：";
            } else if (队伍 == 队伍.蓝) {
                消息 += $"[{蓝色str}蓝方{默认色str}] {发送者} ：";
            } else if (队伍 == 队伍.系统) {
                消息 += $"[{绿色str}系统{默认色str}] {发送者} ：";
            }
            消息 += 内容;
            消息框.AddText(消息);
            下帧(() => {
                消息滚动区.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
            });
        }

        导弹飞行数据 获取导弹数据信息(int 编号, GameObject 导弹物体) {
            var 导弹数据 = new 导弹飞行数据();
            导弹数据.i = 编号;
            导弹数据.tp = 导弹物体.GetComponent<导弹>().类型;
            导弹数据.p = 导弹物体.transform.position.To向量3();
            导弹数据.d = 导弹物体.transform.rotation.To向量4();
            导弹数据.v = 导弹物体.GetComponent<Rigidbody>().velocity.To向量3();
            导弹数据.r = 导弹物体.GetComponent<Rigidbody>().angularVelocity.To向量3();
            return 导弹数据;
        }
        public void 渲染导弹锁定圈(导弹 数据) {
            当前选定导弹 = 数据;
        }
        public void 隐藏导弹锁定圈() {
            当前选定导弹 = null;
        }

        public void 死亡界面() {
            准备界面();
            死亡界面背景.SetActive(true);
        }
    }
}