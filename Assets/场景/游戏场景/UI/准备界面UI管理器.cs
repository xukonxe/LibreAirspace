using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TGZG;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;
using TMPro;
using System.Linq;

public class 准备界面UI管理器 : MonoBehaviour {
    public void 初始化(Action<准备界面UI管理器> onInit) => onInit(this);
    public 小地图管理器 小地图;
    public Button 进入游戏按钮;
    public Action<载具配置, 队伍配置> On确定;
    [Header("油量、挂载")]
    public TMP_Dropdown 载具选择框;
    public Slider 油量选择框;
    public TextMeshProUGUI 油时长文本;
    public List<TMP_Dropdown> 挂载选择框;
    List<载具类型> 可选载具;
    (TimeSpan 底油时间, TimeSpan 满油时间) 可用油数据;
    List<List<挂载类型>> 可用挂载数据;
    float 油量比例 => (float)((载具配置.油量 - 可用油数据.底油时间) / (可用油数据.满油时间 - 可用油数据.底油时间));
    载具配置 载具配置;
    [Header("队伍、出生点")]
    public TMP_Dropdown 队伍选择框;
    public TMP_Dropdown 机场选择框;
    public TMP_Dropdown 机库选择框;
    List<队伍> 可选队伍 = new();
    队伍配置 队伍配置 = new();

    void Start() {
        进入游戏按钮.onClick.AddListener(delegate {
            On确定?.Invoke(载具配置, 队伍配置);
        });
    }
    void Update() {
    }
    public void 加载小地图(Func<List<玩家小地图信息>> Datafrom, 地图管理器 地图 = null) {
        if (地图 != null) 小地图.地图 = 地图;
        小地图.DataFrom = Datafrom;
        小地图.拍摄();
    }
    public void 加载可选载具(List<载具类型> 可选载具) {
        this.可选载具 = 可选载具;
        选择载具(可选载具.First());
    }
    //油量、挂载
    //选择载具后，初始化油量、挂载
    public void 选择载具(载具类型 类型) {
        可用油数据 = 载具模板管理器.获取可用油量(类型);
        可用挂载数据 = 载具模板管理器.获取挂载数据(类型);
        初始化载具配置(类型);
    }
    public void 初始化载具配置(载具类型 选择的类型) {
        载具配置 = new 载具配置 {
            类型 = 选择的类型,
            油量 = 可用油数据.底油时间,
            挂载 = new()
        };
        载具选择框.ClearOptions();
        载具选择框.AddOptions(可选载具.Select(t => t.ToString()).ToList());
        载具选择框.onValueChanged.RemoveAllListeners();
        载具选择框.onValueChanged.AddListener(delegate {
            选择载具((载具类型)载具选择框.value);
        });

        油量选择框.maxValue = 1;
        油量选择框.minValue = 0;

        油量选择框.value = 油量比例;
        调整油量(油量比例);
        油量选择框.onValueChanged.RemoveAllListeners();
        油量选择框.onValueChanged.AddListener(delegate {
            调整油量(油量选择框.value);
        });
        for (int i = 0; i < 挂载选择框.Count; i++) {
            挂载选择框[i].ClearOptions();
            挂载选择框[i].AddOptions(可用挂载数据[i].Select(t => t.ToString()).ToList());
            挂载选择框[i].onValueChanged.RemoveAllListeners();
            挂载选择框[i].onValueChanged.AddListener(delegate {
                载具配置.挂载[i] = (挂载类型)挂载选择框[i].value;
            });
        }
    }

    public void 调整油量(float 比例) {
        var 底油 = 可用油数据.底油时间;
        var 顶油 = 可用油数据.满油时间;
        var 比例油 = new TimeSpan(0, 0, (int)((顶油.TotalSeconds - 底油.TotalSeconds) * 比例));
        TimeSpan 实际油 = 底油 + 比例油;
        油时长文本.text = 实际油.ToString("c");
        载具配置.油量 = 实际油;
    }
    public bool 显示 => gameObject.activeSelf;
    //队伍、出生点
    //选择队伍后，其他选项都选择第一个
    public void 加载可选队伍(List<队伍> 可选队伍) {
        this.可选队伍 = 可选队伍;
        选择队伍(可选队伍.First());
        初始化队伍选择UI();
    }
    public void 初始化队伍选择UI() {
        队伍选择框.ClearOptions();
        队伍选择框.AddOptions(可选队伍.Select(t => t.ToString()).ToList());
        队伍选择框.onValueChanged.RemoveAllListeners();
        队伍选择框.onValueChanged.AddListener(delegate {
            选择队伍(可选队伍[队伍选择框.value]);
        });

        机场选择框.ClearOptions();
        机场选择框.AddOptions(小地图.地图.出生区域列表.Select(t => t.出生区名称).ToList());
        机场选择框.onValueChanged.RemoveAllListeners();
        机场选择框.onValueChanged.AddListener(delegate {
            选择机场(小地图.地图.出生区域列表[机场选择框.value]);
        });

        机库选择框.ClearOptions();
        机库选择框.AddOptions(队伍配置.机场.出生点列表.Select(t => t.出生点名称).ToList());
        机库选择框.onValueChanged.RemoveAllListeners();
        机库选择框.onValueChanged.AddListener(delegate {
            选择出生点(队伍配置.机场.出生点列表[机库选择框.value]);
        });
    }
    public void 选择队伍(队伍 队伍) {
        队伍配置.队伍 = 队伍;
        选择机场(小地图.地图.出生区域列表.First());
    }
    public void 选择机场(载具出生区域 机场) {
        队伍配置.机场 = 机场;
        选择出生点(队伍配置.机场.出生点列表.First());
    }
    public void 选择出生点(载具出生点 出生点) {
        队伍配置.出生点 = 出生点;
    }

    public void 打开() {
        gameObject.SetActive(true);
    }
    public void 关闭() {
        gameObject.SetActive(false);
    }
}
public struct 载具配置 {
    public 载具类型 类型;
    public TimeSpan 油量;
    public List<挂载类型> 挂载;
}
public class 队伍配置 {
    public 队伍 队伍;
    public 载具出生区域 机场;
    public 载具出生点 出生点;
}
namespace 战雷革命 {
    public static class 载具模板管理器 {
        public static Dictionary<载具类型, GameObject> 载具预制体 = new();
        public static GameObject 加载(载具类型 类型) {
            初始化载具(类型);
            return UnityEngine.Object.Instantiate(载具预制体[类型]);
        }
        public static void 初始化载具(载具类型 类型) {
            if (!载具预制体.ContainsKey(类型)) {
                载具预制体[类型] = 加载载具(类型);
                if (载具预制体[类型] == null)
                    "载具加载错误：载具预制体为空".logerror();
                if (载具预制体[类型].GetComponent<油量管理器>() == null)
                    "载具加载错误：油量管理器为空".logerror();
            }
        }
        public static (TimeSpan 底油时间, TimeSpan 满油时间) 获取可用油量(载具类型 类型) {
            初始化载具(类型);
            return 载具预制体[类型].GetComponent<油量管理器>().获取可用油量();
        }
        public static List<List<挂载类型>> 获取挂载数据(载具类型 类型) {
            初始化载具(类型);
            var 所有挂载 = 载具预制体[类型].transform.GetComponentsInChildren<挂点指示器>();
            var 消除重复 = 所有挂载.Select(t => t.允许挂载.ToHashSet().ToList());
            return 消除重复.ToList();//
        }
        static GameObject 加载载具(载具类型 类型) {
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
    }
}