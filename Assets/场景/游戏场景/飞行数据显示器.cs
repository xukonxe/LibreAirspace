using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;
using System;

public class 飞行数据显示器 : MonoBehaviour {
    飞行仪表数据 data;
    void Start() {

    }
    void Update() {
        gameObject.SetText(
                    $"速度：{data.速度:F0} km/h" + Environment.NewLine +
                    $"马赫: {data.马赫:F1}" + Environment.NewLine +
                    $"高度：{data.高度:F0} m" + Environment.NewLine +
                    $"攻角：{data.攻角:F2}°" + Environment.NewLine +
                    $"节流阀: {data.节流阀:F0}%" + Environment.NewLine +
                    $"襟翼: {data.襟翼:F0}%" + Environment.NewLine +
                    $"刹车: {(data.刹车 ? 红色str + "打开" : "关闭")}"
                    );

    }
    public void 刷新(飞行仪表数据 data) {
        this.data = data;
    }
}
