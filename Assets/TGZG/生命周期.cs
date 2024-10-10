using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using XLua;
using System;
using System.Threading;

namespace TGZG {
    public static partial class 公共空间 {
        public static Action 每帧事件 = () => { };
        [Obsolete("请使用初始化每帧(GameObject 临时物体)")]
        public static void 初始化每帧(GameObject 临时物体) {
            临时物体.AddComponent<生命周期>();
        }
        public static void 初始化每帧() {
            if (GameObject.Find("Main").GetComponent<生命周期>() == null) {
                GameObject.Find("Main").AddComponent<生命周期>();
            }
        }
        public static void 每帧(Action action) {
            主线程(() => {
                每帧事件 += action;
            });
        }
        public static void 取消每帧(Action action) {
            主线程(() => {
                每帧事件 -= action;
            });
        }

        public static Action 下帧事件 = () => { };
        [Obsolete("请使用初始化下帧(GameObject 临时物体)")]
        public static void 初始化下帧(GameObject 临时物体) {
            临时物体.AddComponent<生命周期>();
        }
        public static void 初始化下帧() {
            if (GameObject.Find("Main").GetComponent<生命周期>() == null) {
                GameObject.Find("Main").AddComponent<生命周期>();
            }
        }
        public static void 下帧(Action action) {
            主线程(() => {
                下帧事件 += action;
            });
        }
        public static void 取消下帧(Action action) {
            主线程(() => {
                下帧事件 -= action;
            });
        }
    }
    public class 生命周期 : MonoBehaviour {
        public void Start() {

        }
        public void Update() {
            每帧事件?.Invoke();
            下帧事件?.Invoke();
            下帧事件 = () => { };
        }
    }
}