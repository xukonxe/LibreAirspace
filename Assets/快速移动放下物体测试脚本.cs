using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 快速移动放下物体测试脚本 : MonoBehaviour {
    public GameObject 子物体;
    [Range(0, 100)]
    public float 速度 = 10;
    [Range(0, 200)]
    public float 视距 = 10;

    public bool 放下赋值速度 = false;
    bool 释放 = false;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
        //rb.AddForce(Vector3.forward * 速度, ForceMode.VelocityChange);
    }

    // Update is called once per frame
    void Update() {
        rb.velocity = Vector3.forward * 速度;
        //主摄像机跟随本物体
        Camera.main.transform.position = transform.position + Vector3.right * 视距;
        Camera.main.transform.LookAt(transform);
        //按下空格键时，快速移动到下方并放下物体
        if (!释放) {


        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (!释放) {
                释放 = true;
                子物体.transform.parent = null;
                子物体.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                if (放下赋值速度) {
                    子物体.GetComponent<Rigidbody>().velocity = rb.velocity;
                }
            } else {
                释放 = false;
                子物体.transform.parent = transform;
                子物体.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                子物体.transform.localPosition = new Vector3(0, -1.75f, 0);
            }
        }
    }
}
