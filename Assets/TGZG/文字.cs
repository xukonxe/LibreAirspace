using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using XLua;
using UnityEngine.SceneManagement;

/// 此文件需要引用Tencent.Xlua插件的54-v2.2.16版本
namespace TGZG {
    public static partial class 公共空间 {
        public static string 默认色str = "</color>";
        public static string 黑色str = "<color=#000000>";
        public static string 蓝色str = "<color=#0000FF>";
        public static string 淡蓝色str = "<color=#286795>";
        public static string 暗蓝色str = "<color=#0000A0>";
        public static string 浅蓝色str = "<color=#ADD8E6>";
        public static string 红色str = "<color=#FF0000>";
        public static string 橙色str = "<color=#FFA500>";
        public static string 黄色str = "<color=#FFFF00>";
        public static string 绿色str = "<color=#00FF00>";
        public static string 紫色str = "<color=#FF00FF>";
        public static string 白色str = "<color=#FFFFFF>";
        public static string 灰白色str = "<color=#C0C0C0>";
        public static string 灰色str = "<color=#808080>";
        public static string 金色str = "<color=#D9BE19>";//标准金色
        public static string 凡红色str = "<color=#bd3b3b>";
        public static string 凡绿色str = "<color=#40bd40>";
        public static string 字色(string hex) => $"<color=#{hex}>";
        public static string 字号(int 数值) => $"<size={数值}>";
        public static string 默认字号 = "</size>";

        public static byte[] ToBytes(this string X) {
            return System.Text.Encoding.UTF8.GetBytes(X);
        }
    }
}