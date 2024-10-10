using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
namespace 战雷革命 {
    public static partial class 公共空间 {

    }
    public class 摄像机跟随 : MonoBehaviour {
        [Range(-100, 100)]
        public float 视角距离 = -10;
        void Start() {

        }
        void Update() {
            Camera.main.transform.position = transform.position + new Vector3(0, 1, 视角距离);
        }
    }
}
