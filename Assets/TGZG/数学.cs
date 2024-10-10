using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using XLua;
using System.Diagnostics;
using System;

namespace TGZG {
    public static partial class 公共空间 {
        public static double Pow(this double 底数, double 指数) {
            return System.Math.Pow(底数, 指数);
        }
        public static double 绝对值(this double 数) {
            return System.Math.Abs(数);
        }
        public static float 绝对值(this float 数) {
            return System.Math.Abs(数);
        }
        public static double 符号(this double 数) {
            return System.Math.Sign(数);
        }
        public static float 符号(this float 数) {
            return System.Math.Sign(数);
        }
        public static double 趋近1(this double v) {
            return v / (1 + v);
        }
        public static float 趋近1(this float v) {
            return v / (1 + v);
        }
        public static Quaternion ToQuaternion(this Vector4 v) {
            return new Quaternion(v.x, v.y, v.z, v.w);
        }
        public static double 期望修正(this double 值, double 期望, double 修正力) {
            if (修正力 < 0) throw new System.Exception("修正力必须非负");
            return 修正力 * Math.Sign(期望 - 值);
        }
        public static double 递减期望修正(this double 值, double 期望, double 修正力) {
            if (修正力 < 0) throw new System.Exception("修正力必须非负");
            var 绝对差 = (值 - 期望).绝对值();
            if (绝对差 > 修正力)
                return 值.期望修正(期望, 修正力);
            else
                return 值.期望修正(期望, 绝对差 / 2);
        }
        public static float 期望修正(this float 值, float 期望, float 修正力) {
            return (float)((double)值).期望修正((double)期望, (double)修正力);
        }
        public static float 递减期望修正(this float 值, float 期望, float 修正力) {
            return (float)((double)值).递减期望修正((double)期望, (double)修正力);
        }
    }
}