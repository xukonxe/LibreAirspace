using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;

public class AIM9E控制脚本 : MonoBehaviour {
    [Range(-100, 100)]
    public float 视角距离 = 10;
    void Start() {

    }
    void Update() {
        Camera.main.transform.position = transform.position + new Vector3(0, 1, 视角距离);

        if (Input.GetKeyDown(KeyCode.LeftAlt)) {
            "启动".log();
            GetComponent<导弹>().启动();
        }
        //按下空格后，发射导弹
        if (Input.GetKeyDown(KeyCode.Space)) {
            "发射".log();
            GetComponent<导弹>().发射();
        }
        //按下ctrl后重置
        if (Input.GetKey(KeyCode.LeftControl)) {
            "重置".log();
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().rotation = Quaternion.identity;
            GetComponent<导弹>().动力.重置();
            transform.position = new Vector3(0, 17.3f, 0);
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        //wasd控制翼面
        if (Input.GetKey(KeyCode.W)) {
            GetComponent<导弹>().导引头.翼面控制器.控制导弹翼面(new(0, -45));
        }
        if (Input.GetKey(KeyCode.S)) {
            GetComponent<导弹>().导引头.翼面控制器.控制导弹翼面(new(0, 45));
        }
        if (Input.GetKey(KeyCode.A)) {
            GetComponent<导弹>().导引头.翼面控制器.控制导弹翼面(new(-45, 0));
        }
        if (Input.GetKey(KeyCode.D)) {
            GetComponent<导弹>().导引头.翼面控制器.控制导弹翼面(new(45, 0));
        }
    }
}
