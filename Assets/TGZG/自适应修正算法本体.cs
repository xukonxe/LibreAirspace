using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;

namespace TGZG {
    public class 自适应修正控制器 {
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

        List<double> 最大加速度 = new() { 0 };

        double 回正修正(double 差值, double 本帧时间) {
            抖动和回正修正量 = 0;
            var 加速度 = (上帧速度 - 速度) / 本帧时间;
            if (上帧输出 is 1) {
                最大加速度.Add(加速度);
            }
            if (上帧输出 is -1) {
                最大加速度.Add(加速度);
            }
            if (最大加速度.Count > 15) {
                最大加速度.RemoveAt(0);
            }
            double 目标路程 = 0;
            if (运动方向 is 运动方向.向上) {
                目标路程 = Math.Pow(速度, 2) / 最大加速度.Average();
            } else {
                目标路程 = Math.Pow(速度, 2) / 最大加速度.Average();
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
        public double 差值修正系数 = 0.4;
        public double 偏移修正速率 = 0.01;
        public int 自适应远离中心抑制步进帧数间隔 = 10;

        public double 抑制步进步进 = 0.01;
        public double 回正步进步进 = 0.01;

        public double 最大抑制修正步进 = 0.1;
        public double 最大抖动修正量 = 2;
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