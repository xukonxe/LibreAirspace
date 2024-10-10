using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;

using System.Diagnostics;

namespace TGZG {
    public static partial class 公共空间 {
        public static double Pow(this double 底数, double 指数) {
            return System.Math.Pow(底数, 指数);
        }
        public static double 绝对值(this double 数) {
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
        public static Vector2 To双轴角度(this Vector3 本地目标点) {
            var Y轴角度 = Vector3.Angle(new Vector3(0, 本地目标点.y, 本地目标点.z), Vector3.forward) * Mathf.Sign(本地目标点.y);
            var X轴角度 = Vector3.Angle(new Vector3(本地目标点.x, 0, 本地目标点.z), Vector3.forward) * Mathf.Sign(本地目标点.x);
            return new Vector2(X轴角度, Y轴角度);
        }
    }
}