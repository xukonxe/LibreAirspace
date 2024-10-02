using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using TGZG;
using static 战雷革命.公共空间;
using 战雷革命;
using TMPro;
using static UnityEngine.Object;

public class 计分板界面UI管理器 : MonoBehaviour {
    public void 初始化(Action<计分板界面UI管理器> onInit) => onInit(this);
    public string 模板路径 => "Assets/场景/游戏场景/UI/计分板项模板.prefab";
    GameObject 计分板项模板 => 加载资源<GameObject>(模板路径);
    GameObject 计分板项文字模板 => 计分板项模板.Find("计分板项文字模板");

    public GameObject 蓝队列表UI;
    public GameObject 红队列表UI;

    public 计分项管理器 蓝队项管理器;
    public 计分项管理器 红队项管理器;

    void Start() {


    }
    void Update() {

    }
    public bool 显示 => gameObject.activeSelf;
    public void 打开() {
        gameObject.SetActive(true);
    }
    public void 关闭() {
        gameObject.SetActive(false);
    }
    public void 更新数据(计分板数据 蓝队计分数据, 计分板数据 红队计分数据) {
        if (蓝队项管理器 == null || 红队项管理器 == null) {
            蓝队项管理器 = new(计分板项模板, 计分板项文字模板, 蓝队列表UI);
            红队项管理器 = new(计分板项模板, 计分板项文字模板, 红队列表UI);
        }
        蓝队项管理器.更新数据(蓝队计分数据);
        红队项管理器.更新数据(红队计分数据);
    }
}
public class 计分项管理器 {
    List<GameObject> 队列表 = new();
    public GameObject 队列表UI;
    public GameObject 计分板项模板;
    public GameObject 计分板项文字模板;
    public 计分项管理器(GameObject 计分板项模板, GameObject 计分板项文字模板, GameObject 队列表UI) {
        this.计分板项模板 = 计分板项模板;
        this.计分板项文字模板 = 计分板项文字模板;
        this.队列表UI = 队列表UI;
    }
    public void 创建项() {
        var 项物体 = Instantiate(计分板项模板);
        项物体.SetParent(队列表UI);
        队列表.Add(项物体);
    }
    public void 修改项(int 索引, string[] 文字) {
        var 项物体 = 队列表[索引];
        var 文字数量 = 项物体.transform.childCount;
        for (int i = 0; i < 文字.Length; i++) {
            if (i < 文字数量) {
                项物体.transform.GetChild(i).GetComponent<TextMeshProUGUI>().text = 文字[i];
            } else {
                var 文字物体 = Instantiate(计分板项文字模板);
                文字物体.GetComponent<TextMeshProUGUI>().text = 文字[i];
                文字物体.SetParent(项物体);
            }
        }
    }
    public void 删除项(int 索引) {
        var 项物体 = 队列表[索引];
        Destroy(项物体);
    }
    public void 更新数据(计分板数据 队计分数据) {
        int 总项数 = 队计分数据.列数据.Count() + 1;
        int 当前项数 = 队列表.Count;
        if (当前项数 < 总项数) {
            int 缺少项数 = 总项数 - 当前项数;
            for (int i = 0; i < 缺少项数; i++) {
                创建项();
            }
        }
        if (当前项数 > 总项数) {
            int 多余项数 = 当前项数 - 总项数;
            for (int i = 0; i < 多余项数; i++) {
                删除项(i);
            }
        }
        修改项(0, 队计分数据.列定义);
        for (int i = 1; i < 总项数; i++) {
            修改项(i, 队计分数据.列数据[i].Select(x => x.ToString()).ToArray());
        }
    }
}
