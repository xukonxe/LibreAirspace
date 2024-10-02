using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TGZG;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

public class 导弹翼面控制 : MonoBehaviour {
    public Transform 前上;
    public Transform 前下;
    public Transform 前左;
    public Transform 前右;

    public Transform 后上;
    public Transform 后下;
    public Transform 后左;
    public Transform 后右;

    public float 偏角倍数 = 1;
    [Range(0, 90)]
    public float 最大俯仰角度 = 60;
    [Range(0, 90)]
    public float 最大控速修正角度 = 15;

    [Range(-90, 90)]
    public float 俯仰;
    [Range(-90, 90)]
    public float 偏航;

    public float 角度累计帧数 = 10;
    public float 角度累计乘数 = 1;
    void Start() {

    }
    void Update() {

    }
    Queue<float> x角度队列 = new Queue<float>();
    Queue<float> y角度队列 = new Queue<float>();
    public void 控制导弹翼面(Vector2 方向) {

        俯仰 = 方向.y;
        偏航 = 方向.x;

        //var rb = GetComponent<Rigidbody>();
        //var 速度方向 = rb.velocity.normalized;
        //var 速度方向_本地 = transform.InverseTransformDirection(速度方向);
        //var Y轴速度 = 速度方向_本地.y;
        //var X轴速度 = 速度方向_本地.x;
        //俯仰 += Mathf.Clamp(-Y轴速度 * 100 * 2, -最大控速修正角度, 最大控速修正角度);
        //偏航 += Mathf.Clamp(-X轴速度 * 100 * 2, -最大控速修正角度, 最大控速修正角度);

        //时间累计算法，计算时间上累计的角度
        

        
        if (俯仰 is < 3 and > -3) {
            俯仰 = 方向.y * 偏角倍数;
        } else if (俯仰 is < 6 and > -6) {
            俯仰 = 方向.y * 偏角倍数 / 1.5f;
        } else {
            俯仰 = 方向.y * 偏角倍数 / 3;
        }

        if (偏航 is < 3 and > -3) {
            偏航 = 方向.x * 偏角倍数;
        } else if (偏航 is < 6 and > -6) {
            偏航 = 方向.x * 偏角倍数 / 1.5f;
        } else {
            偏航 = 方向.x * 偏角倍数 / 3;
        }


        x角度队列.Enqueue(方向.x);
        if (x角度队列.Count > 角度累计帧数) {
            x角度队列.Dequeue();
            var 平均值 = x角度队列.Sum() / x角度队列.Count;
            var 修正值 = 平均值 * 角度累计乘数;

            //$"偏航平均值： {平均值} 角度乘数： {角度累计乘数}".log();
            //$"导引头转向： {方向.x}".log();
            //$"偏航修正值： {修正值}".log();
            偏航 += 修正值;
        }


        y角度队列.Enqueue(方向.y);
        if (y角度队列.Count > 角度累计帧数) {
            y角度队列.Dequeue();
            var 平均值 = y角度队列.Sum() / y角度队列.Count;
            var 修正值 = 平均值 * 角度累计乘数;
            俯仰 += 修正值;
        }


        //令俯仰和偏航在-45到45之间
        俯仰 = Mathf.Clamp(俯仰, -最大俯仰角度, 最大俯仰角度);
        偏航 = Mathf.Clamp(偏航, -最大俯仰角度, 最大俯仰角度);

        //度数低时，反馈大。度数高时，反馈小。反馈永远在90-90之间。


        前上.localRotation = Quaternion.Euler(0, 偏航, -90);
        前下.localRotation = Quaternion.Euler(0, 偏航, 90);
        前左.localRotation = Quaternion.Euler(-俯仰, 0, 0);
        前右.localRotation = Quaternion.Euler(-俯仰, 0, 180);

        //为rb赋予旋转力
        //rb.AddRelativeTorque(new Vector3(-俯仰 * 偏角倍数, 偏航 * 偏角倍数, 0));




    }
}
