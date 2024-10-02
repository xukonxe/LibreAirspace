using PID;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using 战雷革命;
namespace PID {
    //PID:
    //P:差值修正
    //I:时间修正
    //D:趋势修正
    public class 累差修正 {
        public float 累计时间 = 0.5f;
        public float 累计乘数 = 0.05f;
        public float 累计最大修正 = 0.3f;
        public float 累计回正力 = 0.1f;
        List<float> 累计表 = new List<float>();
        float 累计 = 0;
        public float 计算累差(float 差值, float 本帧时间) {
            //每帧累计俯仰差值
            累计表.Add(差值);
            //超过累计时间后，取出最早的
            if (累计表.Count > (int)(累计时间 / 本帧时间)) {
                //双取出避免无限增长
                累计表.RemoveAt(0);
                累计表.RemoveAt(0);
            }
            //避免空集
            if (累计表.Count != 0) {
                累计 += 累计表.Average() * 累计乘数;
            }
            累计 = Mathf.Clamp(累计, -累计最大修正, 累计最大修正);
            if (累计 > 0) {
                累计 -= 累计回正力;
            } else if (差值 < 0) {
                累计 += 累计回正力;
            }
            return 累计;
        }

        float 差分 = 0;
        public float 差分乘数 = 0.05f;
        public float 差分最大修正 = 0.3f;
        public float 差分回正力 = 0.02f;
        float[] 差分表;

        float[] 累计差分(float[] 数据) {
            float[] 差分 = new float[数据.Length - 1];
            for (int i = 0; i < 差分.Length; i++) {
                差分[i] = 数据[i + 1] - 数据[i];
            }
            return 差分;
        }
        //差分的目的是令差分尽可能趋于0，避免突变
        public float 差分修正(int 差分平均数量) {
            差分表 = 累计差分(累计表.ToArray());
            if (差分表.Length < 差分平均数量) return 0;
            var 差分值 = 差分表.ToArray()[^(差分平均数量 - 1)..^0];
            var 平均值 = 差分值.Average();
            //回正

            //如果平均值大于0，说明正在增长，修正应该为负
            //如果平均值小于0，说明正在减少，修正应该为正
            平均值 = -平均值;
            差分 += 平均值 * 差分乘数;
            差分 = Mathf.Clamp(差分, -差分最大修正, 差分最大修正);
            if (差分 > 0) {
                差分 -= 差分回正力;
            } else if (平均值 < 0) {
                差分 += 差分回正力;
            }
            return 差分;
        }
    }
}
public class 飞控 : MonoBehaviour {
    public 翼面控制 翼面;

    public float 临界角 = 10f;
    public float 中心临界角 = 5f;
    [Header("俯仰修正")]
    public float 俯仰累计最大修正 = 0.5f;
    public float 俯仰累计时间 = 0.5f;
    public float 俯仰累计乘数 = 0.05f;
    public float 俯仰累计回正 = 0.1f;

    public float 俯仰差分最大修正 = 0.3f;
    public int 俯仰差分平均数量 = 5;
    public float 俯仰差分乘数 = 0.05f;
    public float 俯仰差分回正 = 0.02f;
    [Header("偏航修正")]
    public float 偏航累计最大修正 = 0.5f;
    public float 偏航累计时间 = 0.5f;
    public float 偏航累计乘数 = 0.05f;
    public float 偏航累计回正 = 0.1f;

    public float 偏航差分最大修正 = 0.3f;
    public int 偏航差分平均数量 = 5;
    public float 偏航差分乘数 = 0.05f;
    public float 偏航差分回正 = 0.02f;

    [Header("俯仰")]
    [Range(-1, 1)]
    public float 俯仰累计修正 = 0;
    [Range(-1, 1)]
    public float 俯仰差分修正 = 0;
    [Range(-1, 1)]
    public float 俯仰输出 = 0;
    [Header("偏航")]
    [Range(-1, 1)]
    public float 偏航累计修正 = 0;
    [Range(-1, 1)]
    public float 偏航差分修正 = 0;
    [Range(-1, 1)]
    public float 偏航输出 = 0;
    [Header("滚转")]
    [Range(-1, 1)]
    public float 滚转输出 = 0;

    累差修正 俯仰累计 = new 累差修正();
    累差修正 偏航累计 = new 累差修正();
    Vector3 目标点;
    Vector3 本地目标点;
    float 角度差;

    void FixedUpdate() {
        目标点 = GetComponent<摄像机跟随1>().光标指示物体.transform.position;
        本地目标点 = transform.InverseTransformPoint(目标点).normalized * 5;
        角度差 = Vector3.Angle(transform.forward, 目标点 - transform.position);

        俯仰累计.累计最大修正 = 俯仰累计最大修正;
        俯仰累计.累计时间 = 俯仰累计时间;
        俯仰累计.累计乘数 = 俯仰累计乘数;
        俯仰累计.累计回正力 = 俯仰累计回正;
        俯仰累计.差分最大修正 = 俯仰差分最大修正;
        俯仰累计.差分乘数 = 俯仰差分乘数;
        俯仰累计.差分回正力 = 俯仰差分回正;

        偏航累计.累计最大修正 = 偏航累计最大修正;
        偏航累计.累计时间 = 偏航累计时间;
        偏航累计.累计乘数 = 偏航累计乘数;
        偏航累计.累计回正力 = 偏航累计回正;
        偏航累计.差分最大修正 = 偏航差分最大修正;
        偏航累计.差分乘数 = 偏航差分乘数;
        偏航累计.差分回正力 = 偏航差分回正;

        GetComponent<翼面控制>().飞控接口(飞控算法());
    }

    (float 俯仰, float 偏航, float 滚转) 飞控算法() {
        var 差值 = 计算差值();
        //累计、差分修正
        俯仰累计修正 = 俯仰累计.计算累差(差值.俯仰, Time.fixedDeltaTime);
        俯仰差分修正 = 俯仰累计.差分修正(俯仰差分平均数量);
        偏航累计修正 = 偏航累计.计算累差(差值.偏航, Time.fixedDeltaTime);
        偏航差分修正 = 偏航累计.差分修正(偏航差分平均数量);
        //中心临界角内不使用差值原值
        if (角度差 < 中心临界角) {
            差值.俯仰 = 0;
            差值.俯仰 = 0;
        }
        //计算最终输出
        俯仰输出 = Mathf.Clamp(差值.俯仰 + 俯仰累计修正 + 俯仰差分修正, -1, 1);
        偏航输出 = Mathf.Clamp(差值.偏航 + 偏航累计修正 + 偏航差分修正, -1, 1);
        滚转输出 = Mathf.Clamp(差值.滚转, -1, 1);
        return (俯仰输出, 偏航输出, 滚转输出);
    }

    (float 俯仰, float 偏航, float 滚转) 计算差值() {
        float 俯仰 = 0, 偏航 = 0, 滚转 = 0;

        俯仰 = -Mathf.Clamp(本地目标点.y, -1f, 1f);
        偏航 = Mathf.Clamp(本地目标点.x, -1f, 1f);
        //计算滚转
        var 指向滚转 = Mathf.Clamp(本地目标点.x, -1f, 1f);
        var 滚转滚转 = transform.right.y;
        var 切换比例 = Mathf.InverseLerp(0f, 临界角, 角度差);//0回正1滚转
        滚转 = Mathf.Lerp(滚转滚转, 指向滚转, 切换比例);

        俯仰 = Mathf.Clamp(俯仰, -1, 1);
        偏航 = Mathf.Clamp(偏航, -1, 1);
        滚转 = Mathf.Clamp(滚转, -1, 1);
        return (俯仰, 偏航, 滚转);
    }
}
