using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using XLua;
using System.Diagnostics;

/// 此文件需要引用Tencent.Xlua插件的54-v2.2.16版本
namespace TGZG {
    public static partial class 公共空间 {
        public static void Log(this object 消息) {
            消息.log();
        }
        public static void log(this object 消息) {
            UnityEngine.Debug.Log(消息);
        }
        public static void logwarring(this object 消息) {
            UnityEngine.Debug.LogWarning(消息);
        }
        public static void logerror(this object 消息) {
            UnityEngine.Debug.LogError(消息);
        }
        public static double GetKbs(this string str) {
            //先将字符串转换成byte数组
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            return bytes.GetKbs();
        }
        public static double GetKbs(this byte[] bytes) {
            //计算字节数组的长度
            double length = bytes.Length;
            double kb = length / 1024;
            return kb;
        }
    }
}