using PID;
using System;
using System.Collections.Generic;
using System.Linq;
using TGZG;
using UnityEngine;
using 战雷革命;
namespace PID {
    //PID:
    //P:差值修正
    //I:偏移修正
    //D:抖动抑制修正
    
}
public class 飞控 : MonoBehaviour {
    public 翼面控制 翼面;

    public float 临界角 = 10f;
    public float 中心临界角 = 5f;

    [Range(-1, 1)]
    public float 俯仰输出 = 0;
    [Range(-1, 1)]
    public float 偏航输出 = 0;
    [Range(-1, 1)]
    public float 滚转输出 = 0;

    自适应修正控制器 俯仰控制器 = new 自适应修正控制器();
    自适应修正控制器 偏航控制器 = new 自适应修正控制器();

    Vector3 目标点;
    Vector3 本地目标点;
    float 角度差;

    void Start() {
        俯仰控制器.实际值控制方法 += (v) => 俯仰输出 = (float)v;
        偏航控制器.实际值控制方法 += (v) => 偏航输出 = (float)v;
    }
    void FixedUpdate() {
        目标点 = GetComponent<摄像机跟随1>().光标指示物体.transform.position;
        本地目标点 = transform.InverseTransformPoint(目标点).normalized * 5;
        角度差 = Vector3.Angle(transform.forward, 目标点 - transform.position);

        GetComponent<翼面控制>().飞控接口(飞控算法());
    }
    (float 俯仰, float 偏航, float 滚转) 飞控算法() {
        float 俯仰期望坐标 = 本地目标点.y, 偏航期望坐标 = 本地目标点.x, 滚转期望坐标 = 0;

        俯仰控制器.Tick(俯仰期望坐标, Time.deltaTime);
        偏航控制器.Tick(偏航期望坐标, Time.deltaTime);

        return (俯仰输出, 偏航输出, 滚转输出);
    }

    //(float 俯仰, float 偏航, float 滚转) 计算差值() {
    //    float 俯仰 = 0, 偏航 = 0, 滚转 = 0;

    //    俯仰 = -Mathf.Clamp(本地目标点.y, -1f, 1f);
    //    偏航 = Mathf.Clamp(本地目标点.x, -1f, 1f);
    //    //计算滚转
    //    var 指向滚转 = Mathf.Clamp(本地目标点.x, -1f, 1f);
    //    var 滚转滚转 = transform.right.y;
    //    var 切换比例 = Mathf.InverseLerp(0f, 临界角, 角度差);//0回正1滚转
    //    滚转 = Mathf.Lerp(滚转滚转, 指向滚转, 切换比例);

    //    俯仰 = Mathf.Clamp(俯仰, -1, 1);
    //    偏航 = Mathf.Clamp(偏航, -1, 1);
    //    滚转 = Mathf.Clamp(滚转, -1, 1);
    //    return (俯仰, 偏航, 滚转);
    //}
}
