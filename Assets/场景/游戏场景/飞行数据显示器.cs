using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;
using System;

public class 飞行数据显示器 : MonoBehaviour {
    public double 速度;
    public double 马赫;
    public double 高度;
    public double 攻角;
    public double 节流阀;
    public double 襟翼;
    public bool 刹车;
    void Start() {

    }
    void Update() {
        gameObject.SetText(
                    $"速度：{速度:F0} km/h" + Environment.NewLine +
                    $"马赫: {马赫:F1}" + Environment.NewLine +
                    $"高度：{高度:F0} m" + Environment.NewLine +
                    $"攻角：{攻角:F2}°" + Environment.NewLine +
                    $"节流阀: {节流阀:F0}%" + Environment.NewLine +
                    $"襟翼: {襟翼:F0}%" + Environment.NewLine +
                    $"刹车: {(刹车 ? 红色str + "打开" : "关闭")}"
                    );

    }
}
