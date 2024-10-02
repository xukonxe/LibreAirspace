using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using UnityEngine.SceneManagement;

/// 此文件需要引用Tencent.Xlua插件的54-v2.2.16版本
namespace TGZG {
    public static partial class 公共空间 {
        public static void 切换场景(string 场景名) {
            SceneManager.LoadScene(场景名);
        }
        public static void 鼠标隐藏() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        public static void 鼠标显示() {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}