using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class 子弹 : MonoBehaviour {
    Vector3 发射位置;
    Stopwatch 定时器;
    public 枪射指示器 来源;
    void Start() {
        定时器 = Stopwatch.StartNew();
        发射位置 = transform.position;
    }
    void Update() {
        //一分钟、五千米外销毁
        var 飞行时间 = 定时器.ElapsedMilliseconds;
        var 飞行距离 = Vector3.Distance(transform.position, 发射位置);
        if (飞行时间 > 1 * 60 * 1000 || 飞行距离 > 5000) {
            Destroy(gameObject);
        }
    }
}
