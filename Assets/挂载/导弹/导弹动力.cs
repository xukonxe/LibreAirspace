using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class 导弹动力 : MonoBehaviour {

    [Header("一级发动机")]
    public float 前摇1 = 0;
    public float 推力1;
    public float 时间1;
    bool 一级启动 = false;
    Stopwatch 一级计时 = new Stopwatch();

    [Header("二级发动机")]
    public bool 连续启动2 = false;
    public float 前摇2 = 0;
    public float 推力2;
    public float 时间2;
    bool 二级启动 = false;
    Stopwatch 二级计时 = new Stopwatch();

    [Header("三级发动机")]
    public bool 连续启动3 = false;
    public float 前摇3 = 0;
    public float 推力3;
    public float 时间3;
    bool 三级启动 = false;
    Stopwatch 三级计时 = new Stopwatch();

    Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate() {
        if (一级启动) {
            if (一级计时.Elapsed.TotalSeconds < 时间1) {
                rb.AddForce(transform.forward * 推力1);
            } else {
                关闭一级();
                if (连续启动2) 启动二级();
            }
        }
        if (二级启动) {
            if (二级计时.Elapsed.TotalSeconds < 时间2) {
                rb.AddForce(transform.forward * 推力2);
            } else {
                关闭二级();
                if (连续启动3) 启动三级();
            }
        }
        if (三级启动) {
            if (三级计时.Elapsed.TotalSeconds < 时间3) {
                rb.AddForce(transform.forward * 推力3);
            } else {
                关闭三级();
            }
        }
    }
    public void 启动一级() {
        一级启动 = true;
        一级计时.Start();
    }
    public void 关闭一级() {
        一级启动 = false;
        一级计时.Stop();
    }
    public void 启动二级() {
        二级启动 = true;
        二级计时.Start();
    }
    public void 关闭二级() {
        二级启动 = false;
        二级计时.Stop();
    }
    public void 关闭三级() {
        三级启动 = false;
        三级计时.Stop();
    }
    public void 启动三级() {
        三级启动 = true;
        三级计时.Start();
    }
    public void 重置() {
        关闭一级();
        关闭二级();
        关闭三级();
        一级计时.Reset();
        二级计时.Reset();
        三级计时.Reset();
    }
}
