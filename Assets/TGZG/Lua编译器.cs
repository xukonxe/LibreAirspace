using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using XLua;

/// 此文件需要引用Tencent.Xlua插件的54-v2.2.16版本
namespace TGZG {
    public static partial class 公共空间 {
        public static void Lua文件执行(this string 路径, Dictionary<string, object> l) {
            var A = new LuaEnv();
            foreach (var item in l) {
                A.Global.Set(item.Key, item.Value);
            }
            byte[] bytes = System.IO.File.ReadAllBytes(路径);
            string luaScript = System.Text.Encoding.UTF8.GetString(bytes);
            A.DoString(luaScript);
        }
        public static void Lua执行(this string 代码) {
            var A = new LuaEnv();
            A.DoString(代码);
        }
    }
}