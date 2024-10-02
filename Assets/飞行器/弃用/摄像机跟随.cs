using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
namespace 战雷革命 {
    public static partial class 公共空间 {
       
    }
    public class 摄像机跟随 : MonoBehaviour {
        public Vector3 跟踪点向量 = new Vector3(0, 0, 0);
        void Start() {

        }
        void Update() {
            Vector3 飞机朝向 = transform.forward;
            print(飞机朝向.ToJson());


            Camera.main.transform.LookAt(transform.position); 
        }
    }
}
