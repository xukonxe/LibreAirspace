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
namespace PID {
    //PID:
    //P:差值修正
    //I:偏移修正
    //D:抖动抑制修正
    public class 自适应回归控制器 {
        public 控制器参数 参数;
        public double 差值;
        public event Action<double> 实际值控制方法;

        public bool 使用差值修正 = true;
        public bool 使用偏移修正 = true;
        public bool 使用抑制和回正修正 = true;

        public double 差值修正输出;
        public double 偏移修正输出;
        public double 抑制和回正输出;
        public double 输出;

        double 速度 = 0;

        double 上帧差值 = 0;
        double 上帧速度 = 0;
        double 上帧输出 = 0;
        姿态状况 上帧姿态 = 姿态状况.中心;
        姿态状况 姿态 = 姿态状况.中心;
        运动方向 运动方向 = 运动方向.不变;

        public void Tick(double 差值, double 本帧时间) {
            //速度
            速度 = (差值 - 上帧差值) / 本帧时间;
            //姿态状况
            if (差值.绝对值() - 上帧差值.绝对值() > 0)
                姿态 = 姿态状况.远离中心;
            else
                姿态 = 姿态状况.靠近中心;
            //差值变化即为本帧差值-上帧差值
            运动方向 = (差值 - 上帧差值) switch {
                > 0 => 运动方向.向上,
                < 0 => 运动方向.向下,
                0 => 运动方向.不变,
                double.NaN => throw new Exception("NaN"),
            };
            //计算输出
            差值修正输出 = 使用差值修正 ? 弹簧修正(差值) : 0;
            偏移修正输出 = 使用偏移修正 ? 偏移修正(差值, 本帧时间) : 0;
            抑制和回正输出 = 使用抑制和回正修正 ? 抑制和回正修正(差值, 本帧时间) : 0;
            //最后，三个修正值叠加，得到最终输出
            输出 = 差值修正输出 + 偏移修正输出 + 抑制和回正输出;
            输出 = Math.Clamp(输出, -1, 1);
            实际值控制方法?.Invoke(输出);
            //更新上帧差值
            上帧差值 = 差值;
            上帧姿态 = 姿态;
            上帧速度 = 速度;
            上帧输出 = 输出;
        }
        //第一步，根据误差，实时输出差值修正，使差值稳定在某范围内
        double 弹簧修正(double 差值) {
            //差值为正，说明实际值大于期望，输出负修正
            //差值为负，说明实际值小于期望，输出正修正
            var 输出 = -差值 * 参数.差值修正系数;
            return Math.Clamp(输出, -1, 1);
        }
        double 偏移修正量 = 0;
        //第二步，根据误差，输出偏移修正，使差值围绕在中心位置
        double 偏移修正(double 差值, double 本帧时间) {
            //差值为正，说明实际值大于期望，杆量按照一定速率负向增加
            //差值为负，说明实际值小于期望，杆量按照一定速率正向增加
            //速度*本帧时间=本帧修正量
            var 偏移修正量 = -Math.Sign(差值) * 参数.偏移修正速率 * 本帧时间;
            this.偏移修正量 += 偏移修正量;
            this.偏移修正量 = Math.Clamp(this.偏移修正量, -1, 1);
            return this.偏移修正量;
        }
        //第三步，根据误差的变化情况，自适应回归，使差值趋于0
        int 远离中心持续帧数 = 0;
        int 靠近中心持续帧数 = 0;
        double 抖动和回正修正量 = 0;
        double 抑制和回正修正(double 差值, double 本帧时间) {
            //抑制
            if (姿态 is 姿态状况.远离中心) {
                靠近中心持续帧数 = 0;
                //第一帧抑制，首先归零
                if (上帧姿态 is not 姿态状况.远离中心) {
                    抖动和回正修正量 = 0;
                    抑制步进 = 0;
                };
                抖动和回正修正量 += 抑制修正();
                远离中心持续帧数++;
            }
            //回正
            if (姿态 is 姿态状况.靠近中心) {
                远离中心持续帧数 = 0;
                //第一帧回正，归零
                if (上帧姿态 is 姿态状况.远离中心) {
                    抖动和回正修正量 = 0;
                }
                抖动和回正修正量 = 回正修正(差值, 本帧时间);
                靠近中心持续帧数++;
            }

            抖动和回正修正量 = Math.Clamp(抖动和回正修正量, -参数.最大抖动修正量, 参数.最大抖动修正量);
            return 抖动和回正修正量;
        }
        public double 抑制步进 = 0;
        double 抑制修正() {
            double 修正量 = 0;
            if (远离中心持续帧数 < 参数.自适应远离中心抑制步进帧数间隔) {
                //持续远离，说明抑制不足，步进。
                抑制步进 += 参数.抑制步进步进;
            }
            if (运动方向 is 运动方向.向上) {
                修正量 -= 抑制步进;
            }
            if (运动方向 is 运动方向.向下) {
                修正量 += 抑制步进;
            }
            抑制步进 = Math.Clamp(抑制步进, 0, 参数.最大抑制修正步进);
            return 修正量;
        }

        double 向上最大加速度 = 0;
        double 向下最大加速度 = 0;

        double 回正修正(double 差值, double 本帧时间) {
            抖动和回正修正量 = 0;
            var 加速度 = (上帧速度 - 速度) / 本帧时间;
            if (上帧输出 is 1) {
                向上最大加速度 = 加速度;
            }
            if (上帧输出 is -1) {
                向下最大加速度 = 加速度;
            }
            double 目标路程 = 0;
            if (运动方向 is 运动方向.向上) {
                目标路程 = Math.Pow(速度, 2) / 向下最大加速度;
            } else {
                目标路程 = Math.Pow(速度, 2) / 向上最大加速度;
            }
            $"差值：{差值:F2} 目标路程：{目标路程:F2} ".Log();
            if (差值.绝对值() <= 目标路程.绝对值()) {
                var 其他阶段修正 = (差值修正输出 + 偏移修正输出);
                if (运动方向 is 运动方向.向上) {
                    抖动和回正修正量 = -1 - 其他阶段修正;
                } else {
                    抖动和回正修正量 = 1 - 其他阶段修正;
                }
            }
            return 抖动和回正修正量;
        }
    }
    public class 控制器参数 {
        public double 差值修正系数;
        public double 偏移修正速率;
        public int 自适应远离中心抑制步进帧数间隔;

        public double 抑制步进步进;
        public double 回正步进步进;

        public double 最大抑制修正步进;
        public double 最大抖动修正量;
    }
    public enum 姿态状况 {
        靠近中心,
        远离中心,
        中心
    }
    public enum 运动方向 {
        向上,
        向下,
        不变
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

    [Header("新算法参数")]
    自适应回归控制器 俯仰控制器 = new 自适应回归控制器();
    自适应回归控制器 偏航控制器 = new 自适应回归控制器();




    Vector3 目标点;
    Vector3 本地目标点;
    float 角度差;
    Vector2 双轴角度差;

    void FixedUpdate() {
        目标点 = GetComponent<摄像机跟随1>().光标指示物体.transform.position;
        本地目标点 = transform.InverseTransformPoint(目标点).normalized * 5;
        角度差 = Vector3.Angle(transform.forward, 目标点 - transform.position);
        双轴角度差 = 本地目标点.To双轴角度();

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
    //(float 俯仰, float 偏航, float 滚转) 飞控算法() {


    //    return (0,0,0);
    //}

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
