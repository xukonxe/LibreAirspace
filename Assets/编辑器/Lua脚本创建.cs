using UnityEngine;
using UnityEditor;
using System.IO;
//只在编辑模式下执行
#if UNITY_EDITOR
public class Lua脚本创建 : Editor {
    [MenuItem("Assets/Create/Lua脚本")]
    public static void CreateLuaFileHere() {
        //选择文件路径和文件名
        string filePath = EditorUtility.SaveFilePanel("创建Lua脚本", "Assets", "NewLuaScript.lua", "lua");
        if (filePath == "") {
            return;
        }
        Debug.Log("创建Lua脚本：" + filePath);
        //创建文件并写入内容
        File.WriteAllText(filePath, "print('Hello, Lua!')\n-- Your Lua code here");
        AssetDatabase.Refresh();
    }
}
#endif