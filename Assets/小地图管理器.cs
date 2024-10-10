using System.Collections;
using System.Collections.Generic;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace 战雷革命 {
    public static partial class 公共空间 {
        public static Dictionary<小地图项图标, string> 小地图图标路路径 = new() {
            [小地图项图标.载具_红] = "Assets/素材/小地图贴图/载具_红.png",
            [小地图项图标.载具_蓝] = "Assets/素材/小地图贴图/载具_蓝.png",
            [小地图项图标.主玩家载具_红] = "Assets/素材/小地图贴图/主玩家载具_红.png",
            [小地图项图标.主玩家载具_蓝] = "Assets/素材/小地图贴图/主玩家载具_蓝.png",
        };
        public static IEnumerable<T3> 逐个比对<T1, T2, T3>(this IEnumerable<T1> 列1, IEnumerable<T2> 列2, Func<T1, T2, T3> 匹配, Func<T2, T3> 不足, Func<T1, T3> 多余) {
            List<Func<T3>> 延迟筛选 = new();
            var 缓存列1 = 列1.Select(t => t);
            var 缓存列2 = 列2.Select(t => t);
            var max = Math.Max(缓存列1.Count(), 缓存列2.Count());
            for (var i = 0; i < max; i++) {
                var 列1项 = 缓存列1.ElementAtOrDefault(i);
                var 列2项 = 缓存列2.ElementAtOrDefault(i);
                Func<T3> 匹配项 = (列1项, 列2项) switch {
                    (not null, not null) => () => 匹配(列1项, 列2项),
                    (null, not null) => () => 不足(列2项),
                    (not null, null) => () => 多余(列1项),
                    _ => throw new Exception("匹配错误：列表可能已更改"),
                };
                延迟筛选.Add(匹配项);
            }
            return 延迟筛选.Select(项 => 项()).Where(t => t is not null);
        }
    }
    public class 小地图管理器 : MonoBehaviour {
        Camera 地图摄像机;
        public 地图管理器 地图;
        List<小地图_玩家位置标识> 所有玩家位置标识 = new();
        public Func<List<玩家小地图信息>> DataFrom;
        GameObject 标识区 => gameObject;
        float 小地图宽度 => 标识区.GetComponent<RectTransform>().sizeDelta.x;
        public RawImage 图片;
        void Start() {
            初始化主线程();
            if (DataFrom == null) "警告：未设置数据源".logwarring();
            图片.material = 拍摄();
        }
        void FixedUpdate() {
            //每帧:获取玩家位置信息，更新小地图标识。
            //标识数量不足时创建新标识，多余时销毁多余标识。
            if (DataFrom is null) return;
            var 玩家位置数据 = DataFrom.Invoke();
            var 处理后的位置标识 = 玩家位置数据.逐个比对(所有玩家位置标识,
                    匹配: (info, 标识) => {
                        标识.位置 = info.位置;
                        标识.旋转 = info.旋转;
                        标识.队伍 = info.队伍;
                        标识.主玩家 = info.主玩家;
                        return 标识.应用(地图.宽度, 小地图宽度);
                    },
                    不足: 标识 => {
                        标识.Destroy();
                        return null;
                    },
                    多余: info => {
                        var 标识 = info.To标识().应用(地图.宽度, 小地图宽度, 标识区);
                        所有玩家位置标识.Add(标识);
                        return 标识;
                    })
               .ToList();
            所有玩家位置标识 = 处理后的位置标识;
        }
        public Material 拍摄() {
            if (地图 == null) {
                "警告：目标地图为空".logwarring();
                return null;
            }
            var 小地图纹理 = new RenderTexture(256, 256, 24);
            var 小地图材质 = new Material(Shader.Find("Unlit/Texture"));
            小地图材质.mainTexture = 小地图纹理;
            var 地图大小 = 地图.宽度;
            地图摄像机 = new GameObject("地图摄像机").AddComponent<Camera>();
            地图摄像机.cullingMask = 1;
            地图摄像机.orthographic = true;
            地图摄像机.orthographicSize = 地图大小 / 2;
            地图摄像机.targetTexture = 小地图纹理;
            地图摄像机.farClipPlane = 100000;
            地图摄像机.transform.position = new Vector3(地图大小 / 2, 15000, 地图大小 / 2);
            地图摄像机.transform.rotation = Quaternion.Euler(90, 0, 0);
            地图摄像机.Render();
            下帧(() => {
                地图摄像机.targetTexture = null;
                地图摄像机.gameObject.Destroy();
            });
            图片.material = 小地图材质;
            return 小地图材质;
        }
    }
    public struct 玩家小地图信息 {
        public Vector2 位置;
        public float 旋转;
        public 载具类型 载具;
        public 队伍 队伍;
        public bool 主玩家;
        public 小地图_玩家位置标识 To标识(小地图_玩家位置标识 原有 = null) {
            var 标识 = 原有 ?? new 小地图_玩家位置标识();
            标识.位置 = 位置;
            标识.旋转 = 旋转;
            标识.队伍 = 队伍;
            标识.主玩家 = 主玩家;
            return 标识;
        }
    }
    public enum 小地图项图标 {
        载具_红,
        载具_蓝,
        主玩家载具_红,
        主玩家载具_蓝,
    }
    public class 小地图显示项 {
        public Vector2 位置;
        public float 旋转;
        public Image 图标;
        public string 图标路径;
        public void Destroy() {
            图标.gameObject.Destroy();
        }
    }
    public class 小地图_玩家位置标识 : 小地图显示项 {
        public 队伍 队伍;
        public bool 主玩家;
        public 小地图_玩家位置标识 应用(float 地图宽高, float 小地图大小, GameObject 父 = null) {
            GameObject 标识物体;
            图标路径 = (队伍, 主玩家) switch {
                (队伍.红, false) => 小地图图标路路径[小地图项图标.载具_红],
                (队伍.蓝, false) => 小地图图标路路径[小地图项图标.载具_蓝],
                (队伍.红, true) => 小地图图标路路径[小地图项图标.主玩家载具_红],
                (队伍.蓝, true) => 小地图图标路路径[小地图项图标.主玩家载具_蓝],
                _ => null
            };

            if (图标路径 is null) return this;
            if (图标 is null) {
                标识物体 = new GameObject("小地图玩家位置标识");
                标识物体.AddComponent<RectTransform>();
                图标 = 标识物体.AddComponent<Image>();
                if (父 != null) 标识物体.SetParent(父);
            } else {
                标识物体 = 图标.gameObject;
            }
            标识物体.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            标识物体.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 0f);
            标识物体.GetComponent<RectTransform>().localPosition = 小地图大小 * 位置 / 地图宽高;
            标识物体.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 10);
            标识物体.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 旋转);
            图标.sprite = 加载资源<Texture2D>(图标路径).ToSprite();
            return this;
        }
    }
}
