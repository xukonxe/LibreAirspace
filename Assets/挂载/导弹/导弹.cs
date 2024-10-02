using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;
using System;

public class 导弹 : MonoBehaviour {
    public 挂载类型 类型;
    public float 阻力系数;
    public List<Collider> 自身碰撞箱 = new();
    [HideInInspector]
    public 导弹动力 动力;
    [HideInInspector]
    public 导引头 导引头;
    [HideInInspector]
    public TrailRenderer 尾迹;


    public bool 已启动 => 导引头.已启动;
    Rigidbody rb;

    public Action On爆炸;
    public Action OnStart;
    void Start() {
        foreach (var i in 自身碰撞箱) {
            i.isTrigger = true;
        }
        rb = GetComponent<Rigidbody>();
        rb.drag = 阻力系数;
        动力 = gameObject.GetComponent<导弹动力>();
        导引头 = gameObject.GetComponent<导引头>();
        尾迹 = gameObject.Find("轨迹").GetComponent<TrailRenderer>();
        OnStart?.Invoke();
        GetComponent<爆炸控制器>().爆炸前 += On爆炸;
    }
    void Update() {

    }
    public void 启动() {
        导引头.启动();
    }
    public void 发射() {
        尾迹.enabled = true;
        动力.启动一级();
        导引头.开始发射计时();
        foreach (var i in 自身碰撞箱) {
            i.isTrigger = false;
        }
    }
    public void 停止() {
        导引头.停止();
    }
    public void 爆炸() {
        导引头.爆炸控制器.爆炸();
    }
}
