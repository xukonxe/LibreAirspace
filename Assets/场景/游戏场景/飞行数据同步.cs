using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;
using System;
namespace 战雷革命 {
    public static partial class 公共空间 {
        public static Vector3 ToVector3(this float[] v) {
            return new Vector3(v[0], v[1], v[2]);
        }
        public static Quaternion ToQuaternion(this float[] v) {
            return new Quaternion(v[0], v[1], v[2], v[3]);
        }
        public static float[] To向量3(this Vector3 v) {
            return new float[] { v.x, v.y, v.z };
        }
        public static float[] To向量4(this Quaternion v) {
            return new float[] { v.x, v.y, v.z, v.w };
        }
    }
    public class 飞行数据同步 : MonoBehaviour {
        载具类型 载具类型;
        GameObject 玩家名称显示;
        bool 初始化;
        public 玩家进入数据 登录数据 {
            get {
                if (!初始化) throw new InvalidOperationException("请先调用初始化名称UI方法");
                return new 玩家进入数据() {
                    n = gameObject.name,
                    tp = 载具类型
                };
            }
            set {
                if (!初始化) throw new InvalidOperationException("请先调用初始化名称UI方法");
                载具类型 = value.tp;
                gameObject.name = value.n;
            }
        }
        public 玩家世界数据 世界数据 {
            get {
                if (!初始化) throw new InvalidOperationException("请先调用初始化名称UI方法");
                return 获取数据();
            }
            set {
                if (!初始化) throw new InvalidOperationException("请先调用初始化名称UI方法");
                更新数据(value);
            }
        }
        public void 更新数据(玩家世界数据 数据) {
            var transform = gameObject.transform;
            var rb = gameObject.GetComponent<Rigidbody>();
            transform.position = 数据.p.ToVector3();
            transform.rotation = 数据.d.ToQuaternion();
            rb.velocity = 数据.v.ToVector3();
            rb.angularVelocity = 数据.r.ToVector3();
        }
        public 玩家世界数据 获取数据() {
            return new 玩家世界数据() {
                p = gameObject.transform.position.To向量3(),
                d = gameObject.transform.rotation.To向量4(),
                v = gameObject.GetComponent<Rigidbody>().velocity.To向量3(),
                r = gameObject.GetComponent<Rigidbody>().angularVelocity.To向量3()
            };
        }
        public void 初始化名称UI() {
            初始化 = true;
            玩家名称显示 = Instantiate(加载资源<GameObject>("Assets/场景/游戏场景/玩家名称.prefab"));
            玩家名称显示.SetParent(GameObject.Find("Canvas"));
            玩家名称显示.transform.SetAsFirstSibling();
        }
        void Update() {
            玩家名称显示.映射到世界物体(gameObject, 屏幕偏移: new(0, 30));
            var 距离 = Vector3.Distance(Camera.main.transform.position, gameObject.transform.position);
            玩家名称显示.SetText($"{gameObject.name} ({载具类型.ToString()})\n{距离 / 1000:F2}Km");
        }
        void OnDestroy() {
            玩家名称显示.Destroy();
        }
    }
}
