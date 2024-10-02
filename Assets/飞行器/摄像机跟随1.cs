using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;
using UnityEngine.UI;

namespace 战雷革命 {
    public static partial class 公共空间 {
        public static Dictionary<挂载类型, string> 导弹预设体 = new() {
            { 挂载类型.AIM9E, "Assets/挂载/导弹/AIM9E/AIM-9E.prefab"}
        };
        public static Vector3 坐标系转化(this Vector3 坐标, Vector3 原坐标系, Vector3 目标坐标系) {
            // 确保Z轴向量归一化
            Vector3 原Z轴 = 原坐标系.normalized;
            Vector3 新Z轴 = 目标坐标系.normalized;
            Vector3 新X轴 = Vector3.Cross(原Z轴, 新Z轴).normalized;
            Vector3 新Y轴 = Vector3.Cross(新X轴, 原Z轴).normalized;

            // 创建变换矩阵
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.SetColumn(0, 新X轴); // X轴
            matrix.SetColumn(1, 新Y轴); // Y轴
            matrix.SetColumn(2, 原Z轴); // Z轴

            // 应用变换
            return matrix.MultiplyPoint(坐标);
        }
        public static Sprite ToSprite(this Texture2D texture) {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
    public class 摄像机跟随1 : MonoBehaviour {
        public bool 暂停视角控制 = false;

        public float 上偏移 = 5;
        public float 后偏移 = 8;

        public GameObject 光标指示物体;//用于确认光标在屏幕上显示的位置;
        public GameObject 光标;
        public GameObject 准星指示物体;//用于确认准星在屏幕上显示的位置;
        public GameObject 准星;
        public GameObject 自由视角指示物体;

        Vector3 速度指向矢量;
        Vector3 载具指向矢量;
        Vector3 光标指向矢量;
        Vector3 自由视角矢量;
        Vector3 摄像机朝向矢量;

        Vector3 上帧角速度 = Vector3.zero;
        Rigidbody 碰撞体;

        public float 光标灵敏度 = 4;
        public float 自由视角灵敏度 = 7;
        public float 跟随速度 = 6;
        public float 俯仰修正乘数 = 2f;
        public float 偏航修正乘数 = 12f;

        public float 角速度计算次数每秒 = 20;
        public float 临界角度 = 5f;

        public float 光标距离 = 10000;
        public float 准星距离 = 10000;

        public List<挂点指示器> 挂点 = new();

        public void 隐藏光标() {
            光标.SetActive(false);
            准星.SetActive(false);
        }
        public void 显示光标() {
            光标.SetActive(true);
            准星.SetActive(true);
        }

        protected void Start() {
            碰撞体 = GetComponent<Rigidbody>();
            //出生时，默认光标朝向为载具的朝向
            光标指向矢量 = gameObject.transform.forward;
            摄像机朝向矢量 = 光标指向矢量;
            //创建光标指示物体
            光标指示物体 = new GameObject("光标指示物体");
            光标指示物体.transform.SetParent(gameObject.transform, false);

            //在canvas下创建一个白色半透明矩形,大小50*50
            光标 = new GameObject("光标");
            光标.transform.SetParent(GameObject.Find("Canvas").transform, false);
            光标.AddComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
            光标.AddComponent<CanvasRenderer>();
            光标.AddComponent<Image>().color = new(255, 255, 255, 1);
            光标.GetComponent<Image>().sprite = 加载资源<Texture2D>("Assets/素材/零碎图片素材/鼠标外环.png").ToSprite();

            //创建准星指示物体
            准星指示物体 = new GameObject("准星指示物体");
            准星指示物体.transform.SetParent(gameObject.transform, false);

            //在canvas下创建一个黑色半透明矩形,大小20*20
            准星 = new GameObject("准星");
            准星.transform.SetParent(GameObject.Find("Canvas").transform, false);
            准星.AddComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
            准星.AddComponent<CanvasRenderer>();
            准星.AddComponent<Image>().color = new(255, 255, 255, 1);
            准星.GetComponent<Image>().sprite = 加载资源<Texture2D>("Assets/素材/零碎图片素材/准星.png").ToSprite();

            自由视角指示物体 = new GameObject("自由视角指示物体");
            自由视角指示物体.transform.SetParent(gameObject.transform, false);

            UI更新光标位置();
            UI更新准星位置();
        }
        // Update is called once per frame
        protected void Update() {
            if (!暂停视角控制) {
                自由视角矢量 = 计算自由视角();
                //如果此帧按下C，那么自由视角初始化（即为光标位置）;
                if (Input.GetKeyDown(KeyCode.C)) {
                    自由视角矢量 = 光标指向矢量;
                }
                //只有不按住C时才更新光标位置()
                if (!Input.GetKey(KeyCode.C)) {
                    光标指向矢量 = 计算光标指向();
                }
                速度指向矢量 = gameObject.GetComponent<Rigidbody>().velocity.normalized;
                载具指向矢量 = gameObject.transform.forward;
            }
            摄像机朝向矢量 = 计算摄像机朝向();
            更新摄像机朝向();
            UI更新光标位置();
            UI更新准星位置();
        }
        protected void FixedUpdate() {
            上帧角速度 = 碰撞体.angularVelocity;
        }
        protected void OnDestroy() {
            光标指示物体.Destroy();
            光标.Destroy();
            准星指示物体.Destroy();
            准星.Destroy();
            自由视角指示物体.Destroy();
        }
        Vector3 计算自由视角() {
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");
            var 灵敏度 = 自由视角灵敏度 / 10;
            Quaternion 水平角度变化 = Quaternion.AngleAxis(x * 灵敏度, Vector3.up);
            Quaternion 垂直角度变化 = Quaternion.AngleAxis(y * 灵敏度, Vector3.Cross(自由视角矢量, Vector3.up));
            Quaternion 角度变化 = 水平角度变化 * 垂直角度变化;
            Vector3 新矢量 = 角度变化 * 自由视角矢量;
            if (Vector3.Angle(新矢量, Vector3.up) >= 1
            &&
            Vector3.Angle(新矢量, Vector3.down) >= 1
            ) {
                return 新矢量;
            } else {
                return 自由视角矢量;
            }
        }
        Vector3 计算摄像机朝向() {
            //计算摄像机朝向
            Quaternion 当前摄像机方向 = Quaternion.FromToRotation(Vector3.up, 摄像机朝向矢量);
            Quaternion 修正方向;
            if (Input.GetKey(KeyCode.C)) {
                修正方向 = Quaternion.FromToRotation(Vector3.up, 自由视角矢量);
            } else {
                修正方向 = Quaternion.FromToRotation(Vector3.up, 光标指向矢量);
            }
            Quaternion 摄像机本帧变化 = Quaternion.SlerpUnclamped(当前摄像机方向, 修正方向, Time.deltaTime * 跟随速度);
            return (当前摄像机方向 * Vector3.up) + (摄像机本帧变化 * Vector3.up);
        }
        Vector3 计算光标指向() {
            //计算光标指向
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");
            var 灵敏度 = 光标灵敏度 / 10;
            Quaternion 水平角度变化 = Quaternion.AngleAxis(x * 灵敏度, Vector3.up);
            Quaternion 垂直角度变化 = Quaternion.AngleAxis(y * 灵敏度, Vector3.Cross(光标指向矢量, Vector3.up));
            Quaternion 角度变化 = 水平角度变化 * 垂直角度变化;
            Vector3 新矢量 = 角度变化 * 光标指向矢量;
            //限制光标角度，避免溢出错误
            if (Vector3.Angle(新矢量, Vector3.up) >= 1
                &&
                Vector3.Angle(新矢量, Vector3.down) >= 1
                ) {
                return 新矢量;
            } else {
                return 光标指向矢量;
            }
        }
        void UI更新光标位置() {
            光标指示物体.transform.position = transform.position + 光标指向矢量 * 光标距离;
            光标.映射到世界物体(光标指示物体);
        }
        void UI更新准星位置() {
            准星指示物体.transform.position = transform.position + 载具指向矢量 * 准星距离;
            准星.映射到世界物体(准星指示物体);
        }
        void 更新摄像机朝向() {
            //应用摄像机位置
            Vector3 上移 = new Vector3(0, 上偏移, 0);
            Vector3 后移 = 摄像机朝向矢量 * -1 * 后偏移;
            Camera.main.transform.position = gameObject.transform.position + 上移 + 后移;
            //应用摄像机注视点
            Vector3 注视点 = gameObject.transform.position + 上移;
            Camera.main.transform.LookAt(注视点);
        }
        void 发射导弹(挂载类型 类型) {
            //查找
            var 匹配挂架 = 挂点.Find(t =>
                t.挂载物体 != null
                && t.挂载物体.GetComponent<导弹>() != null
                && t.挂载物体.GetComponent<导弹>().类型 == 类型
                );
            if (匹配挂架 == null) return;
            var 导弹 = 匹配挂架.挂载物体.GetComponent<导弹>();
            //发射
            导弹.发射();
            匹配挂架.放开挂载();
        }
        //从所有有导弹的挂点中随机选择一个发射
        void 发射导弹() {
            var 挂架 = 挂点.Find(t => t.挂载物体.GetComponent<导弹>() != null);
            if (挂架 == null) return;
            发射导弹(挂架.挂载物体.GetComponent<导弹>().类型);
        }
    }
}
