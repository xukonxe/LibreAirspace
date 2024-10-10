using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;
using System.Linq;
using UnityEngine.UI;
using TMPro;
namespace 战雷革命 {
    public static partial class 公共空间 {
        public static 房间数据类 待加载房间数据;
        public static 玩家进入数据 玩家数据 = new() { tp = default };
        public static GameObject 加载其他玩家飞行器(玩家游玩数据 数据) {
            var 飞行器 = 载具模板管理器.加载(数据.u.tp);
            飞行器.GetComponent<摄像机跟随1>().enabled = false;
            飞行器.GetComponent<飞控>().enabled = false;
            飞行器.GetComponent<翼面控制>().enabled = false;
            var 同步器 = 飞行器.AddComponent<飞行数据同步>();
            同步器.初始化名称UI();
            同步器.登录数据 = 数据.u;
            同步器.世界数据 = 数据.p;
            var 毁伤计算 = 飞行器.GetComponent<毁伤计算>();
            毁伤计算.自动损坏 = false;
            毁伤计算.被击中 += (攻击者, 被击者, 部位, 伤害值, 损坏) => {
                //当攻击者为主视角玩家时，将伤害值发送至服务
                if (攻击者 != 玩家数据.n) return;
                var A = new 击伤信息();
                A.攻击者 = 攻击者;
                A.被攻者 = 被击者;
                A.部位 = 部位;
                A.伤害 = 伤害值;
                信道_游戏.击伤(A);
            };
            var 油量管理器 = 飞行器.GetComponent<油量管理器>();
            油量管理器.设置油量(数据.u.油量);
            return 飞行器;
        }
        public static GameObject 加载玩家飞行器(玩家游玩数据 玩家) {
            var 飞行器 = 载具模板管理器.加载(玩家.u.tp);
            var 同步器 = 飞行器.AddComponent<飞行数据同步>();
            同步器.初始化名称UI();
            同步器.登录数据 = 玩家.u;
            同步器.世界数据 = 玩家.p;
            var 毁伤计算 = 飞行器.GetComponent<毁伤计算>();
            毁伤计算.被击中 += (攻击者, 被击者, 部位, 伤害值, 损坏) => {
                //if (攻击者 == "") return;//忽略碰撞伤害
                //上传损坏

                "损坏！".log();
                if (损坏) {
                    "上传损坏提示".log();
                    $"攻击者：{攻击者}".log();
                    信道_游戏.上传损坏提示(攻击者, 部位);
                }
            };
            var 油量管理器 = 飞行器.GetComponent<油量管理器>();
            油量管理器.设置油量(玩家.u.油量);
            ///var 挂载管理器 = 飞行器.GetComponent<挂点指示器>();
            //挂载管理器.设置挂载(玩家.u.挂载);
            return 飞行器;
        }
        public static GameObject 加载导弹(挂载类型 类型) {
            return UnityEngine.Object.Instantiate(加载资源<GameObject>(导弹预设体[类型]));
        }
    }
    public static class 地图模板管理器 {
        public static Dictionary<string, GameObject> 地图预制体 = new();
        public static bool 验证存在性(string 地图名) {
            if (地图预制体.ContainsKey(地图名)) return true;
            var 地图 = 加载资源<GameObject>($"Assets/地图/{地图名}.prefab");
            if (地图 is not null && 地图.GetComponent<地图管理器>() is not null) {
                地图预制体[地图名] = 地图;
                return true;
            }
            return false;
        }
        public static GameObject 加载(string 类型) {
            if (!验证存在性(类型)) throw new ArgumentException($"地图{类型}不存在");
            return UnityEngine.Object.Instantiate(地图预制体[类型]);
        }
    }
    public class 玩家导弹管理器 {
        public int 编号;
        public 导弹 导弹;
        public Action 爆炸前;
        public 玩家导弹管理器(导弹飞行数据 导弹数据) {
            导弹.导引头.爆炸控制器.爆炸前 += 爆炸前;
            this.编号 = 导弹数据.编号;
            var 导弹对象 = 加载导弹(导弹数据.tp);
            导弹对象.transform.position = 导弹数据.p.ToVector3();
            导弹对象.transform.rotation = 导弹数据.d.ToQuaternion();
            导弹对象.GetComponent<Rigidbody>().velocity = 导弹数据.v.ToVector3();
            导弹对象.GetComponent<Rigidbody>().angularVelocity = 导弹数据.r.ToVector3();
            导弹对象.GetComponent<导弹>().OnStart += () => {
                导弹对象.GetComponent<导弹>().导引头.GetComponent<近炸控制器>().启动近炸 = false;
                导弹对象.GetComponent<导弹>().发射();
            };
        }
        public 玩家导弹管理器(导弹 导弹, int 编号) {
            导弹.导引头.爆炸控制器.爆炸前 += 爆炸前;
            this.编号 = 编号;
            this.导弹 = 导弹;
        }
        public void 爆炸() { }
        public void 损坏(HashSet<部位> 损坏数据) { }
        public void 更新位置(导弹飞行数据 导弹数据) {
            导弹.transform.position = 导弹数据.p.ToVector3();
            导弹.transform.rotation = 导弹数据.d.ToQuaternion();
            导弹.GetComponent<Rigidbody>().velocity = 导弹数据.v.ToVector3();
            导弹.GetComponent<Rigidbody>().angularVelocity = 导弹数据.r.ToVector3();
        }
        public 导弹飞行数据 GetData() {
            var 导弹数据 = new 导弹飞行数据();
            导弹数据.编号 = 编号;
            导弹数据.tp = 导弹.GetComponent<导弹>().类型;
            导弹数据.p = 导弹.transform.position.To向量3();
            导弹数据.d = 导弹.transform.rotation.To向量4();
            导弹数据.v = 导弹.GetComponent<Rigidbody>().velocity.To向量3();
            导弹数据.r = 导弹.GetComponent<Rigidbody>().angularVelocity.To向量3();
            return 导弹数据;
        }
    }
    public class 飞行器管理器 {
        public string 名称;
        public 队伍 队伍;
        public 载具类型 类型 = 载具类型.无;
        public GameObject 飞行器;
        List<Stopwatch> 开枪计时器 = new();
        public List<玩家导弹管理器> 玩家导弹 = new();
        public event Action<飞行仪表数据> 当飞行仪表更新;
        public event Action<玩家游玩数据> 当物理更新;
        public HashSet<部位> 损坏数据 => 飞行器.GetComponent<毁伤计算>().损坏数据;
        public 飞行数据显示器 仪表;
        public bool 主玩家 { get; private set; }
        public 导弹 当前选定导弹 { get; private set; }
        public void 数据更新() {
            当飞行仪表更新?.Invoke(仪表参数());
            当物理更新?.Invoke(ToUpDateData());
        }
        public void 隐藏光标() {
            飞行器.GetComponent<摄像机跟随1>().隐藏光标();
            飞行器.GetComponent<摄像机跟随1>().暂停视角控制 = true;
        }
        public void 显示光标() {
            飞行器.GetComponent<摄像机跟随1>().显示光标();
            飞行器.GetComponent<摄像机跟随1>().暂停视角控制 = false;
        }
        public 飞行器管理器(玩家进入数据 玩家, 地图管理器 当前地图) {
            var 初始玩家数据 = new 玩家游玩数据() {
                u = 玩家,
                p = 当前地图.get出生数据(玩家),
                射 = new int[] { },
                msl = new(),
                损坏 = new(),
            };
            初始化(初始玩家数据, true);
        }
        public 飞行器管理器(玩家游玩数据 玩家, bool 主玩家 = false) {
            初始化(玩家, 主玩家);
        }
        void 初始化(玩家游玩数据 玩家, bool 主玩家) {
            名称 = 玩家.u.n;
            飞行器 = 主玩家 switch {
                true => 加载玩家飞行器(玩家),
                false => 加载其他玩家飞行器(玩家)
            };
            队伍 = 玩家.u.tm;
            类型 = 玩家.u.tp;
        }
        public void 损坏(HashSet<部位> 损坏部位) {
            飞行器.GetComponent<毁伤计算>().损坏(损坏部位);
        }
        public void 被击伤(击伤信息 击伤数据) {
            飞行器.GetComponent<毁伤计算>().伤害(击伤数据);
        }
        public void 同步位置(玩家世界数据 位置) {
            飞行器.GetComponent<飞行数据同步>().世界数据 = 位置;
        }
        public void 射击控制(int[] 射击数据) {
            var 机炮 = 飞行器.FindAll(t => t.tag == "机炮");
            int i = 0;
            foreach (var j in 机炮) {
                if (射击数据 != null && 射击数据.Length > i && 射击数据[i] == 1) {
                    j.GetComponent<枪射指示器>().射击(飞行器.transform);
                }
                i++;
            }
        }
        public List<bool> 本帧射击情况 = new();
        public void 射击() {
            //获取玩家飞行器下所有包含tag：机炮 的GameObject
            var 机炮 = 飞行器.FindAll(t => t.tag == "机炮");
            //遍历机炮，发射子弹
            int num = 0;
            foreach (var i in 机炮) {
                var 指示器 = i.GetComponent<枪射指示器>();
                if (开枪计时器.Count() < num + 1) 开枪计时器.Add(Stopwatch.StartNew());
                if (本帧射击情况.Count < num + 1) 本帧射击情况.Add(false);
                if (开枪计时器[num].ElapsedMilliseconds > (60 * 1000) / 指示器.每分射速) {
                    指示器.射击(飞行器.transform);
                    本帧射击情况[num] = true;
                    开枪计时器[num].Restart();
                }
                num++;
            }
        }
        public void 导弹同步(List<导弹飞行数据> 导弹数据) {
            if (导弹数据 != null) {
                //对所有导弹数据，找到对应的对象，更新数据位置
                foreach (var 导弹 in 导弹数据) {
                    var 导弹对象 = 玩家导弹.FirstOrDefault(t => t.编号 == 导弹.编号);
                    if (导弹对象 == default) continue;//多余信息忽略
                    导弹对象.更新位置(导弹);
                }
            }
        }
        public void 导弹预热或发射(Action<导弹> 预热 = null, Action<导弹> 发射 = null) {
            var 挂点 = 飞行器.GetComponent<摄像机跟随1>().挂点;
            foreach (var i in 挂点) {
                if (i.挂载物体 == null) continue;
                var 导弹 = i.挂载物体.GetComponent<导弹>();
                if (导弹 != null) {
                    if (!导弹.已启动) {
                        导弹.启动();
                        当前选定导弹 = 导弹;
                        预热?.Invoke(导弹);
                    } else {
                        导弹.发射();
                        i.放开挂载();
                        发射?.Invoke(导弹);
                    }
                    return;
                }
            }
        }
        public void 取消导弹预热() {
            var 挂点 = 飞行器.GetComponent<摄像机跟随1>().挂点;
            foreach (var i in 挂点) {
                if (i.挂载物体 != null && i.挂载物体.tag == "导弹") {
                    var 导弹 = i.挂载物体.GetComponent<导弹>();
                    if (导弹.已启动) {
                        导弹.停止();
                        当前选定导弹 = null;
                    }
                }
            }
        }
        public 飞行仪表数据 仪表参数() {
            飞行仪表数据 Data = new();
            //更新参数显示
            var 飞行数据 = 飞行器.GetComponent<飞行数据同步>().世界数据;
            Data.速度 = 飞行数据.v.ToVector3().magnitude * 3.6f;
            Data.马赫 = 飞行数据.v.ToVector3().magnitude / 340;
            Data.高度 = 飞行数据.p[1];
            var 指向角度 = 飞行数据.d.ToQuaternion() * Vector3.forward;
            var 速度方向 = 飞行数据.v.ToVector3().normalized;
            Data.攻角 = Vector3.Angle(速度方向, 指向角度);
            Data.节流阀 = 飞行器.GetComponent<翼面控制>().节流阀 * 100;
            Data.襟翼 = 飞行器.GetComponent<翼面控制>().襟翼 / 飞行器.GetComponent<翼面控制>().襟翼开启值 * 100;
            Data.刹车 = 飞行器.GetComponent<翼面控制>().刹车 == 100f;
            return Data;
        }
        public void 注册仪表(飞行数据显示器 仪表) {
            this.仪表 = 仪表;
            当飞行仪表更新 = t => 仪表.刷新(t);
        }
        public 玩家游玩数据 ToUpDateData() {
            var 玩家同步器 = 飞行器.GetComponent<飞行数据同步>();
            var 毁伤情况 = 飞行器.GetComponent<毁伤计算>();
            //管理玩家发射的导弹
            List<导弹飞行数据> 玩家发射的导弹 = new();
            foreach (var i in 玩家导弹) {
                var 导弹数据 = new 导弹飞行数据();
                导弹数据.编号 = i.编号;
                导弹数据.tp = i.导弹.类型;
                导弹数据.p = i.导弹.transform.position.To向量3();
                导弹数据.d = i.导弹.transform.rotation.To向量4();
                导弹数据.v = i.导弹.GetComponent<Rigidbody>().velocity.To向量3();
                导弹数据.r = i.导弹.GetComponent<Rigidbody>().angularVelocity.To向量3();
                玩家发射的导弹.Add(导弹数据);
            }
            var 数据 = new 玩家游玩数据() {
                u = 获取玩家信息(),
                p = 玩家同步器.世界数据,
                射 = 本帧射击情况.Select(t => t ? 1 : 0).ToArray(),
                msl = 玩家发射的导弹,
                损坏 = 毁伤情况.损坏数据,
            };
            本帧射击情况 = 本帧射击情况.Select(t => t = false).ToList();
            return 数据;
        }
        public 玩家小地图信息 获取地图信息(bool 主玩家 = false) {
            玩家小地图信息 信息 = new();
            信息.主玩家 = 主玩家;
            信息.位置 = new(飞行器.transform.position.x, 飞行器.transform.position.z);
            信息.旋转 = -飞行器.transform.rotation.eulerAngles.y;
            信息.队伍 = 队伍;
            信息.载具 = 类型;
            return 信息;
        }
        public 玩家进入数据 获取玩家信息() {
            玩家进入数据 信息 = 飞行器.GetComponent<飞行数据同步>().登录数据;
            信息.tm = 队伍;
            信息.油量 = 飞行器.GetComponent<油量管理器>().获取当前油量();
            return 信息;
        }
        public void Destroy() {
            飞行器.Destroy();
        }
    }
    public struct 飞行仪表数据 {
        public float 速度;
        public float 马赫;
        public float 高度;
        public float 攻角;
        public float 节流阀;
        public float 襟翼;
        public bool 刹车;
    }
    public class 队伍数据类 {
        public int 红队满 { get; set; }
        public int 蓝队满 { get; set; }
        public int 红队分数 { get; set; }
        public int 蓝队分数 { get; set; }
        public void 更新数据(计分板数据 红队数据, 计分板数据 蓝队数据) {
            蓝队满 = 600;
            红队满 = 600;
            蓝队分数 = 蓝队数据.取列("击杀").Sum(t => (int)t);
            红队分数 = 红队数据.取列("击杀").Sum(t => (int)t);
        }
    }
    public class 导弹预热环管理器 {
        导弹 当前选定导弹;
        public GameObject 外环;
        public GameObject 内环;
        public 导弹预热环管理器(GameObject 外环, GameObject 内环) {
            this.外环 = 外环;
            this.内环 = 内环;
        }
        public void 渲染(导弹 数据) {
            当前选定导弹 = 数据;
        }
        public void 隐藏() {
            当前选定导弹 = null;
        }
        public void 刷新() {
            if (当前选定导弹 != null) {
                外环.SetActive(true);
                内环.SetActive(true);
                var 外环半径角 = 当前选定导弹.导引头.离轴角度;
                var 内环半径角 = 当前选定导弹.导引头.视场;
                var 内环角度 = 当前选定导弹.导引头.导引头转动角度;

                var 外环宽高 = 获取环宽高(外环半径角);
                var 内环宽高 = 获取环宽高(内环半径角);
                外环.GetComponent<RectTransform>().sizeDelta = 外环宽高;
                内环.GetComponent<RectTransform>().sizeDelta = 内环宽高;
                var 外环位置 = 获取环位置(new(0, 0));
                var 内环位置 = 获取环位置(内环角度);
                内环.GetComponent<RectTransform>().position = new Vector2(内环位置.x, 内环位置.y);
                外环.GetComponent<RectTransform>().position = new Vector2(外环位置.x, 外环位置.y);

                if (当前选定导弹.导引头.导引头锁定) {
                    内环.GetComponent<Image>().material.SetColor("_CircleColor", Color.red);
                } else {
                    内环.GetComponent<Image>().material.SetColor("_CircleColor", Color.white);
                }
            } else {
                外环.SetActive(false);
                内环.SetActive(false);
            }
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
    }
    public abstract class 模式管理器 {
        private int _同步频率;
        public int 同步频率 {
            get => _同步频率;
            set {
                _同步频率 = value;
                Time.fixedDeltaTime = 1f / (float)value;
            }
        }
        public 聊天消息管理器 聊天;
        public 地图管理器 当前地图;
        public 界面切换器类 界面切换器;
        public 飞行器管理器 玩家飞行器;
        public 队伍数据类 队伍数据 = new();
        public 导弹预热环管理器 导弹预热环;
        public List<载具类型> 允许载具类型 = new();
        public List<队伍> 允许选择的队伍类型 = new();
        public List<飞行器管理器> 其他玩家飞行器 = new();
        public void 加载数据(房间数据类 数据) {
            加载地图(数据.地图名);
            加载允许载具(数据.可选载具);
            加载允许队伍(数据.可选队伍);
        }
        //数据
        public void 启动同步(int tick) {
            同步频率 = tick;
            //注册：接收游玩数据同步
            信道_游戏.OnRead["同步其他玩家数据"] = t => {
                var 所有其他玩家 = t["其他玩家"].JsonToCS<List<玩家游玩数据>>();
                //数据同步逻辑
                主线程(() => {
                    更新或添加其他玩家(所有其他玩家);
                });
                return null;
            };
            //信道_游戏.OnRead["同步重生"] = t => {
            //    var 同步玩家 = t["玩家"].JsonToCS<玩家游玩数据>();
            //    //删除本地飞行器，自动重新加载心跳包中的飞行器
            //    主线程(() => {
            //        var 飞行器 = 其他玩家飞行器.FirstOrDefault(t => t.名称 == 同步玩家.u.n);
            //        飞行器.Destroy();
            //        飞行器 = new 飞行器管理器(同步玩家, 当前地图);
            //    });
            //    return null;
            //};
            //信道_游戏.OnRead["同步损坏"] = t => {
            //    var 所属玩家 = t["玩家"];
            //    var 损坏数据 = t["数据"].JsonToCS<HashSet<部位>>();
            //    //更新飞行器损坏情况
            //    主线程(() => {
            //        var 飞行器 = 其他玩家飞行器.FirstOrDefault(t => t.名称 == 所属玩家);
            //        if (飞行器 == default) return;
            //        飞行器.损坏(损坏数据);
            //    });
            //    return null;
            //};
            信道_游戏.OnRead["导弹发射"] = t => {
                //创建导弹并设置到对应位置
                var 所属玩家 = t["玩家"];
                var 导弹数据 = t["导弹数据"].JsonToCS<导弹飞行数据>();
                var 编号 = 导弹数据.编号;
                其他玩家飞行器.Find(t => t.名称 == 所属玩家).玩家导弹.Add(new(导弹数据));
                return null;
            };
            信道_游戏.OnRead["导弹爆炸"] = t => {
                var 所属玩家 = t["玩家"];
                var 导弹数据 = t["导弹数据"].JsonToCS<导弹飞行数据>();
                //找到对应的导弹，爆炸并销毁
                主线程(() => {
                    var 导弹 = 其他玩家飞行器.Find(t => t.名称 == 所属玩家)?
                    .玩家导弹.FirstOrDefault(t => t.编号 == 导弹数据.编号);
                    if (导弹 != default) {
                        导弹.爆炸();
                        其他玩家飞行器.Find(t => t.名称 == 所属玩家).玩家导弹.Remove(导弹);
                    }
                });
                return null;
            };
            信道_游戏.OnRead["被击伤"] = t => {
                var 击伤数据 = t["数据"].JsonToCS<击伤信息>();
                主线程(() => {
                    玩家飞行器.被击伤(击伤数据);
                });
                return null;
            };
            信道_游戏.OnRead["击杀提示"] = t => {
                string 被击杀者 = t["被击杀者"];
                主线程(() => {
                    摧毁空中目标提示(被击杀者);
                });
                return null;
            };
            //注册：接收其他消息同步
            信道_游戏.OnRead["聊天消息"] = t => {
                聊天.新增(t["发送者"], t["内容"], Enum.Parse<队伍>(t["队伍"]));
                return null;
            };
            信道_游戏.OnRead["死亡"] = t => {
                主线程(() => {
                    界面切换器.死亡界面();
                });
                return null;
            };
            信道_游戏.OnRead["房间内玩家死亡消息"] = t => {
                var 死亡玩家 = t["玩家"];
                主线程(() => {
                    聊天.新增("系统", $"{红色str}{死亡玩家}{默认色str} 已死亡", 队伍.系统);
                });
                return null;
            };
            信道_游戏.OnRead["更新计分板"] = t => {
                var 蓝队计分数据 = t["蓝队数据"].JsonToCS<计分板数据>();
                var 红队计分数据 = t["红队数据"].JsonToCS<计分板数据>();

                主线程(() => {
                    队伍数据.更新数据(蓝队计分数据, 红队计分数据);
                    界面切换器.计分板.更新数据(蓝队计分数据, 红队计分数据);
                });
                return null;
            };
            //断链处理
            信道_游戏.OnDisconnected += () => {
                界面切换器.服务器断开连接();
                玩家飞行器?.Destroy();
                其他玩家飞行器.ForEach(t => t.Destroy());
            };
        }
        void 更新或添加其他玩家(List<玩家游玩数据> 所有其他玩家) {
            lock (其他玩家飞行器) {
                Action 列表修改事件 = () => { };
                //如果本地有此玩家，但服务器没有，则销毁其飞行器
                foreach (var 此飞行器 in 其他玩家飞行器) {
                    if (!所有其他玩家.Exists(t => t.u.n == 此飞行器.名称)) {
                        此飞行器.Destroy();
                        列表修改事件 += () => {
                            其他玩家飞行器.Remove(此飞行器);
                        };
                    }
                }
                foreach (var 此玩家 in 所有其他玩家) {
                    var 此 = 其他玩家飞行器.Find(t => t.名称 == 此玩家.u.n);
                    //如果本地没有此玩家，则创建其飞行器
                    if (此 == null) {
                        此 = new(此玩家, false);
                        列表修改事件 += () => {
                            其他玩家飞行器.Add(此);
                        };
                    }
                    //如果本地飞行器类型不同，则销毁本地飞行器，创建新飞行器
                    if (此.类型 != 此玩家.u.tp && 此.类型 != 载具类型.无) {
                        重生();
                    }
                    //如果本地损坏比服务器多，那么销毁并创建新飞行器
                    if (此.损坏数据.Count > 此玩家.损坏.Count) {
                        重生();
                    }
                    void 重生(){
                        此.Destroy();
                        此 = new(此玩家, false);
                        列表修改事件 += () => {
                            其他玩家飞行器.Add(此);
                        };
                    }
                    此.队伍 = 此玩家.u.tm;
                    此.同步位置(此玩家.p);
                    此.损坏(此玩家.损坏);
                    此.射击控制(此玩家.射);
                    此.导弹同步(此玩家.msl);
                }
                列表修改事件.Invoke();
            }
        }
        //验证
        public void 验证登录((string 账号, string 密码) 玩家, Action 成功, Action<string> 失败) {
            信道_游戏.WaitFor("登录验证结果", 10, t => {
                var 状态 = t["状态"];
                if (状态 == "成功") {
                    成功?.Invoke();
                } else {
                    失败?.Invoke(t["消息"]);
                }
            });
            信道_游戏.Send(
                ("标题", "验证登录"),
                ("账号", 玩家.账号),
                ("密码", 玩家.密码)
                );

        }
        public void 验证失败(string 错误信息) {

        }
        public void 进入(载具配置 载具, 队伍配置 队伍) {
            //设置进入数据
            玩家数据.tp = 载具.类型;
            玩家数据.油量 = 载具.油量;
            玩家数据.出生点 = (队伍.机场.出生区名称, 队伍.出生点.出生点名称);
            玩家数据.tm = 队伍.队伍;
            玩家数据.挂载 = 载具.挂载.ToArray();
            进入游戏或重生(玩家数据);
        }
        //UI
        public void 加载地图(string 地图名) {
            var 地图管理器 = 地图模板管理器.加载(地图名).GetComponent<地图管理器>();
            if (地图管理器 == null) throw new Exception("地图中不存在地图管理器！");
            当前地图 = 地图管理器;
        }
        public void 加载允许载具(List<载具类型> 允许载具类型) {
            this.允许载具类型 = 允许载具类型;
        }
        public void 加载允许队伍(List<队伍> 允许选择的队伍类型) {
            this.允许选择的队伍类型 = 允许选择的队伍类型;
        }
        public void 初始化准备界面() {
            界面切换器.准备.On确定 = (载具, 队伍) => 进入(载具, 队伍);
            界面切换器.准备.加载小地图(
                () => {
                    var 所有其他玩家 = 其他玩家飞行器
                    .Select(飞行器 => 飞行器.获取地图信息())
                    .ToList();
                    if (玩家飞行器 != null) {
                        所有其他玩家.Add(玩家飞行器.获取地图信息(true));
                    }
                    return 所有其他玩家;
                },
                当前地图);
            界面切换器.准备.加载可选载具(允许载具类型);
            界面切换器.准备.加载可选队伍(允许选择的队伍类型);
        }
        public void 摧毁空中目标提示(string 被击杀者) {
            界面切换器.游戏.提示摧毁空中目标(被击杀者);
        }
        //每帧
        public void 数据更新() {
            信道_游戏.Tick();
            玩家飞行器?.数据更新();
            其他玩家飞行器.ForEach(t => t.数据更新());
        }
        public void UI更新() {
            导弹预热环?.刷新();
        }
        public void 玩家数据更新() {
            if (玩家飞行器 is null) return;
            //机炮
            if (Input.GetAxis("Fire1") > 0)
                玩家飞行器.射击();
            //导弹(由于导弹也是一类飞行物，因此数据收发单独处理)
            if (Input.GetKeyDown(KeyCode.LeftAlt))
                玩家飞行器.导弹预热或发射(
                    预热: 导弹 => 导弹预热环.渲染(导弹),
                    发射: 导弹 => {
                        上传导弹发射(导弹);
                        导弹预热环.隐藏();
                    });
            if (Input.GetKeyUp(KeyCode.LeftControl)) {
                玩家飞行器.取消导弹预热();
                导弹预热环.隐藏();
            }
            //发送数据
            //信道_游戏.发送数据(玩家飞行器.ToUpDateData());

        }
        public void 上传导弹发射(导弹 导弹物体) {
            //本地储存对应编号的导弹
            int 当前编号 = 0;
            if (玩家飞行器.玩家导弹.Count != 0) {
                当前编号 = 玩家飞行器.玩家导弹.Max(t => t.编号) + 1;
            }
            var 此导弹 = new 玩家导弹管理器(导弹物体, 当前编号);
            玩家飞行器.玩家导弹.Add(此导弹);
            此导弹.爆炸前 += () => {
                信道_游戏.导弹爆炸(此导弹.GetData());
                玩家飞行器.玩家导弹.Remove(此导弹);
            };
            //提交本地此编号导弹发射的数据
            信道_游戏.导弹发射(此导弹.GetData());
        }
        public void 鼠标控制() {
            鼠标显示();
            if (玩家飞行器 != null) 玩家飞行器.飞行器.GetComponent<摄像机跟随1>().暂停视角控制 = true;
        }
        public void 鼠标隐藏() {
            鼠标隐藏();
            if (玩家飞行器 != null) 玩家飞行器.飞行器.GetComponent<摄像机跟随1>().暂停视角控制 = false;
        }
        public void 进入游戏或重生(玩家进入数据 玩家) {
            //bool 重生 = 玩家飞行器 != null;
            玩家飞行器?.Destroy();
            玩家飞行器 = null;
            玩家飞行器 = new 飞行器管理器(玩家, 当前地图);
            玩家飞行器.注册仪表(界面切换器.悬浮.飞行数据显示);
            玩家飞行器.当物理更新 += t => {
                信道_游戏.发送数据(t);
                //var A = new 击伤信息();
                //A.攻击者 = "沈伊利1";
                //A.部位 = 部位.身;
                //A.伤害 = 1000;
                //信道_游戏.击伤(A);
            };
            界面切换器.游戏界面();
            //if (重生) {
            //    信道_游戏.发送重生(玩家);
            //}
        }
        public void 退出房间() {
            信道_游戏.清理事件();
            信道_房间列表.清理事件();
            信道_游戏.断开();
            切换场景("主界面");
        }
        public virtual void 初始化UI(游戏场景主程序 数据) {
            界面切换器 = new(this,
                           数据.游戏界面,
                           数据.准备界面,
                           数据.聊天界面,
                           数据.悬浮界面,
                           数据.计分板界面,
                           数据.Esc界面);
            导弹预热环 = new 导弹预热环管理器(数据.游戏界面.导弹外环, 数据.游戏界面.导弹内环);
            界面切换器.悬浮.DataFrom = () => 队伍数据;
        }
    }
    public class 休闲模式管理器 : 模式管理器 {
        public override void 初始化UI(游戏场景主程序 数据) {
            base.初始化UI(数据);
            界面切换器.准备.初始化(t => { });
            界面切换器.聊天.初始化(t => {
                t.清空按钮.onClick.AddListener(() => {
                    聊天.清空();
                });
                t.发送按钮.onClick.AddListener(() => {
                    信道_游戏.发送聊天消息(玩家数据.n, t.输入框.text);
                });
            });
            聊天 = new 聊天消息管理器(界面切换器.聊天);
            界面切换器.悬浮.初始化(t => { });
            界面切换器.计分板.初始化(t => { });
            界面切换器.Esc界面.初始化(t => {
                t.返回主界面按钮.onClick.AddListener(() => {
                    退出房间();
                });
                t.重生按钮.onClick.AddListener(() => {
                    进入游戏或重生(玩家数据);
                });
                t.退出游戏按钮.onClick.AddListener(() => {
                    信道_游戏.断开();
                    Application.Quit();
                });
            });
        }
    }
    public class 竞技模式管理器 : 模式管理器 {

    }
    public class 自定义模式管理器 : 模式管理器 {

    }
    public class 界面切换器类 {
        public 模式管理器 父;
        public 游戏界面UI管理器 游戏;
        public 准备界面UI管理器 准备;
        public 聊天界面UI管理器 聊天;
        public 悬浮界面UI管理器 悬浮;
        public 计分板界面UI管理器 计分板;
        public esc界面UI管理器 Esc界面;
        public 界面切换器类(模式管理器 父,
                游戏界面UI管理器 游戏,
                准备界面UI管理器 准备,
                聊天界面UI管理器 聊天,
                悬浮界面UI管理器 悬浮,
                计分板界面UI管理器 计分板,
                esc界面UI管理器 Esc界面) {
            this.父 = 父;
            this.游戏 = 游戏;
            this.准备 = 准备;
            this.聊天 = 聊天;
            this.悬浮 = 悬浮;
            this.计分板 = 计分板;
            this.Esc界面 = Esc界面;
        }
        public void 服务器断开连接() {

        }
        public void 启闭Esc界面() {
            if (Esc界面.显示) {
                Esc界面.关闭();
                父.玩家飞行器?.显示光标();
                鼠标隐藏();
            } else {
                Esc界面.打开();
                父.玩家飞行器?.隐藏光标();
                鼠标显示();
            }
        }
        public void 游戏界面() {
            Esc界面.关闭();
            计分板.关闭();
            准备.关闭();
            鼠标隐藏();
        }
        public void 计分板启闭() {
            if (计分板.显示) {
                计分板.关闭();
                鼠标隐藏();
                父.玩家飞行器?.显示光标();
            } else {
                计分板.打开();
                父.玩家飞行器?.隐藏光标();
                鼠标显示();
            }
        }
        public void 死亡界面() { }
        public void 准备界面() {
            准备.打开();
        }
        public void 准备界面启闭() {
            if (准备.显示) {
                准备.关闭();
                父.玩家飞行器?.显示光标();
                鼠标隐藏();
            } else {
                准备.打开();
                父.玩家飞行器?.隐藏光标();
                鼠标显示();
            }
        }
    }
    public class 聊天消息管理器 {
        public List<聊天消息条目> 消息 = new();
        public ScrollRect 消息滚动区;
        public TextMeshProUGUI 消息显示;
        public TMP_InputField 聊天输入框;
        public 聊天消息管理器(聊天界面UI管理器 聊天UI) {
            消息滚动区 = 聊天UI.滚动区域;
            消息显示 = 聊天UI.聊天记录;
            聊天输入框 = 聊天UI.输入框;
        }
        public void 新增(string 发送者, string 内容, 队伍 队伍) {
            消息.Add(new(发送者, 内容, 队伍));
            刷新消息();
        }
        public void 清空() {
            消息.Clear();
            刷新消息();
        }
        private void 刷新消息() {
            var 显示数据 = string.Join(
                    Environment.NewLine,
                    消息.Select(t => 格式化(t)));
            消息显示.SetText(显示数据);
            下帧(() => {
                消息滚动区.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
            });
        }
        private string 格式化(聊天消息条目 Data) {
            var 消息 = $"[{DateTime.Now:HH:mm:ss}]";
            switch (Data.队伍) {
                case 队伍.无:
                消息 += $"[{灰色str}无{默认色str}] {Data.发送者} ：{Data.内容}";
                break;
                case 队伍.红:
                消息 += $"[{红色str}红方{默认色str}] {Data.发送者} ：{Data.内容}";
                break;
                case 队伍.蓝:
                消息 += $"[{蓝色str}蓝方{默认色str}] {Data.发送者} ：{Data.内容}";
                break;
                case 队伍.系统:
                消息 += $"[{绿色str}系统{默认色str}] {Data.发送者} ：{Data.内容}";
                break;
            }
            return 消息;
        }
    }
    public struct 聊天消息条目 {
        public DateTime 时间;
        public string 发送者;
        public string 内容;
        public 队伍 队伍;
        public 聊天消息条目(string 发送者, string 内容, 队伍 队伍) {
            this.时间 = DateTime.Now;
            this.发送者 = 发送者;
            this.内容 = 内容;
            this.队伍 = 队伍;
        }
    }



    //当成功连接至游戏服务器时，此场景加载，此脚本加载。
    //此脚本负责管理游戏场景的主要逻辑。
    //已使用中文变量名，因此具体处理逻辑不再赘述。
    //*****************
    //注：由于中文信息密度是英文的3.5倍，因此使用中文变量名后将无需注释。
    //*****************
    public class 游戏场景主程序 : MonoBehaviour {
        [Header("游戏主程序")]
        public 模式管理器 当前模式;
        [Header("UI")]
        public 游戏界面UI管理器 游戏界面;
        public 准备界面UI管理器 准备界面;
        public 聊天界面UI管理器 聊天界面;
        public 悬浮界面UI管理器 悬浮界面;
        public 计分板界面UI管理器 计分板界面;
        public esc界面UI管理器 Esc界面;
        //场景启动时，根据云端传来的房间模式信息，加载相应模式。
        void Start() {
            if (待加载房间数据.模式 is 模式类型.休闲) {
                当前模式 = new 休闲模式管理器();
                当前模式.加载数据(待加载房间数据);
                当前模式.启动同步(待加载房间数据.每秒同步次数);
                当前模式.初始化UI(this);
                当前模式.验证登录(当前登录, 成功: () => {
                    当前模式.初始化准备界面();
                    玩家数据.n = 当前登录.账号;
                }, 失败: t => {
                    当前模式.验证失败(t);
                });
            } else if (待加载房间数据.模式 is 模式类型.竞技) {
                throw new NotImplementedException("竞技模式尚未实现");
            } else if (待加载房间数据.模式 is 模式类型.自定义) {
                throw new NotImplementedException("自定义模式尚未实现");
            } else throw new NotImplementedException("未知模式");
        }
        //每帧调用，用于UI更新
        void Update() {
            当前模式.UI更新();

            //UI按键控制
            if (Input.GetKeyDown(KeyCode.Escape))
                当前模式.界面切换器.启闭Esc界面();
            if (Input.GetKeyUp(KeyCode.Tab))
                当前模式.界面切换器.计分板启闭();
            if (Input.GetKeyDown(KeyCode.M))
                当前模式.界面切换器.准备界面启闭();
        }
        //固定时间调用，用于数据同步
        void FixedUpdate() {
            当前模式.玩家数据更新();
            当前模式.数据更新();
        }
    }
}