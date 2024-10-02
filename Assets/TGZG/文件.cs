using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;

using System;
using System.Linq;

/// 此文件需要引用Tencent.Xlua插件的54-v2.2.16版本
namespace TGZG {
    public static partial class 公共空间 {
        public static bool 文件存在(this string 路径) {
            return System.IO.File.Exists(路径);
        }
        public static void 文件创建(this string 路径) {
            System.IO.File.Create(路径).Close();
        }
        public static void 文件删除(this string 路径) {
            System.IO.File.Delete(路径);
        }
        public static void 文件写入(this string 路径, string 内容) {
            //如果路径不存在就创建路径和文件
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(路径))) {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(路径));
            }
            //写入文件
            System.IO.File.WriteAllText(路径, 内容);
        }
        public static string 文件读取(this string 路径) {
            return System.IO.File.ReadAllText(路径);
        }
    }
}