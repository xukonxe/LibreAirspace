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
      
    }
    public class 摄像机跟随2 : MonoBehaviour {
        public float 飞船动量;
        public Vector3 飞船坐标;
        public Quaternion 飞船方向;//三向旋转
        public Quaternion 镜头方向 {
            get => Camera.main.transform.rotation;
            set => Camera.main.transform.rotation = value;
        }
        public Vector2 鼠标坐标;//Input.mousePosition
        public void Update() {
            var 朝向方向 = 计算朝向(鼠标坐标, 飞船坐标);//Quaternion.LookRotation
            飞船方向 = 调整飞船(飞船方向, 飞船动量, 朝向方向);
            朝向方向 = 调整朝向(镜头方向, 朝向方向);
            镜头方向 = 调整镜头(朝向方向);
            鼠标坐标 = 调整鼠标(朝向方向);
        }
        public static Quaternion 计算朝向(Vector2 鼠标坐标, Vector3 飞船坐标) {
            return Quaternion.LookRotation((计算光标(鼠标坐标) - 飞船坐标).normalized);//减法结果不可能为零，不需要特殊处理
        }
        public static Vector3 计算光标(Vector2 鼠标坐标) { //计算光标所在射线在一万米远之外的坐标
            var A = Camera.main.ScreenPointToRay(鼠标坐标);
            return A.origin + A.direction * 10000; //暂时使用 10000 米远
        }
        public static Quaternion 调整飞船(Quaternion 飞船方向, float 飞船动量, Quaternion 朝向方向) {
            return Quaternion.Lerp(飞船方向, 朝向方向, 2 * Time.deltaTime);//暂时忽视动量，未来添加
        }
        public static Quaternion 调整朝向(Quaternion 镜头方向, Quaternion 朝向方向) {
            return Quaternion.Lerp(镜头方向, 朝向方向, 6 * Time.deltaTime);
        }
        public static Quaternion 调整镜头(Quaternion 朝向方向) {
            return 朝向方向;
        }
        public static Vector2 调整鼠标(Quaternion 朝向方向) {
            return Camera.main.WorldToScreenPoint(朝向方向 * Vector3.forward);
        }
    }
}
