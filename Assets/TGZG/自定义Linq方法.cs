using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;

using System;
using System.Linq;

/// 此文件需要引用Tencent.Xlua插件的54-v2.2.16版本
namespace TGZG {
    public static partial class 公共空间 {
        /// <summary>
        /// 从字典中找到对应项，如果成功，那么移除该项
        /// </summary>
        public static void RemoveKey<T1, T2>(this Dictionary<T1, T2> dict, T1 key) {
            //从字典中找到对应项
            //从字典中移除对应项
            if (dict.ContainsKey(key)) {
                dict.Remove(key);
            }
        }
    }
}