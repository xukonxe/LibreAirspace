using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;

public class 油量管理器 : MonoBehaviour {
    Rigidbody 机身;
    AircraftPhysics 物理;
    public float 满油重量;
    public float 空载质量;
    public float 满油油质量 => 满油重量 - 空载质量;
    public float 每秒消耗质量 => 满油油质量 / (float)满油时间.TotalSeconds;
    [ReadOnly]
    public float 当前油量;
    public float 底油时间秒;
    public float 满油时间秒;
    public TimeSpan 底油时间 => TimeSpan.FromSeconds(底油时间秒);
    public TimeSpan 满油时间 => TimeSpan.FromSeconds(满油时间秒);
    void Start() {

        机身 = GetComponent<Rigidbody>();
        机身.mass = 空载质量;
        物理 = GetComponent<AircraftPhysics>();
    }
    void Update() {
        //处理因载油导致的重量变化
        机身.mass = 空载质量 + 当前油量;
    }
    public (TimeSpan 底油时间, TimeSpan 满油时间) 获取可用油量() => (底油时间, 满油时间);
    public void 设置油量(TimeSpan 目标) {
        if (!(目标 > 底油时间 && 目标 < 满油时间))
            throw new Exception($"油量超出范围{底油时间:hh:mm:ss}-{满油时间:hh:mm:ss}");
        var 目标比例 = (满油时间 - 目标) / (满油时间 - 底油时间);
        当前油量 = 满油油质量 * (float)目标比例;
    }
}
