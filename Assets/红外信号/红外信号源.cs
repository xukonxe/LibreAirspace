using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 红外信号源 : MonoBehaviour {
    public 信号类型 类型;
    public float 尾追强度;
    public float 侧追强度;
    public float 对头强度;
    void Start() {

    }
    void Update() {

    }
    public float 初始强度(Vector3 追踪者位置) {
        //计算追踪者位置和信号源朝向的夹角
        float 角度 = Vector3.Angle(transform.forward, 追踪者位置 - transform.position);
        //根据角度和类型计算强度
        if (角度 is < 45) {
            return 对头强度;
        } else if (角度 is < 135) {
            return 侧追强度;
        } else {
            return 尾追强度;
        }
    }
}
public enum 信号类型 {
    飞行器,
    导弹,
    诱饵弹,
    太阳,
    地面载具,
}


