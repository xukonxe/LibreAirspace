using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;

using System;
using System.Threading;

/// 此文件需要引用Tencent.Xlua插件的54-v2.2.16版本
namespace TGZG {
    public static partial class 公共空间 {
        public static AppThread MainThread;
        public static void 初始化主线程(GameObject 物体 = null) {
            //遍历所有
            if (物体 == null) 物体 = GameObject.FindWithTag("主程序附加于此");
            if (物体 == null) { 
                物体 = new GameObject("自动创建的主程序物体"); 
                物体.tag = "主程序附加于此";
            }
            if (MainThread == null) MainThread = 物体.AddComponent<AppThread>();
        }
        public static void OnMainThread(Action Y) {
            MainThread.Enqueue(Y);
        }
        public static void StartCoroutines(IEnumerator X) {
            MainThread.StartCoroutines(X);
        }
        public static void 后台(WaitCallback A) {
            ThreadPool.QueueUserWorkItem(A);
        }
        public static void 主线程(Action A) {
            OnMainThread(A);
        }
    }
    public class AppThread : MonoBehaviour {
        public Queue<Action> ThreadQueue = new();
        public Action OnQuit;
        public Action OnFoucus;
        public Action OnFoucusOut;
        public void Update() {
            lock (ThreadQueue) {
                while (ThreadQueue.Count > 0) ThreadQueue.Dequeue().Invoke();
            }
        }
        public void OnApplicationQuit() {
            OnQuit?.Invoke();
        }
        public void OnApplicationFocus(bool X) {
            if (X) {
                OnFoucus?.Invoke();
            } else {
                OnFoucusOut?.Invoke();
            }
        }
        public void Enqueue(Action action) {
            lock (ThreadQueue) {
                ThreadQueue.Enqueue(() => StartCoroutine(ActionWrapper(action)));
            }
        }
        IEnumerator ActionWrapper(Action a) {
            a();
            yield return null;
        }
        public void StartCoroutines(IEnumerator coroutine) {
            lock (ThreadQueue) {
                ThreadQueue.Enqueue(() => StartCoroutine(coroutine));
            }
        }
    }
}