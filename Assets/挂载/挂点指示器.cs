using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;

public class 挂点指示器 : MonoBehaviour {
    public Rigidbody 父物体;
    public GameObject 挂载物体;
    public List<挂载类型> 允许挂载;
    float 长 = 5f; // 箭头大小
    float 头长 = 0.2f; // 箭头大小

    void Start() {

    }
    void Update() {
        //使其永远朝向前方
        if (挂载物体 != null) {
            挂载物体.GetComponent<Rigidbody>().rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
            挂载物体.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
        }
    }
    private void FixedUpdate() {

    }
    void OnDrawGizmosSelected() {
        // 计算箭头的方向
        Vector3 方向 = transform.forward;
        Vector3 头 = transform.position + 方向 * 长;

        // 绘制箭头
        Gizmos.color = Color.blue; // 设置箭头颜色
        Gizmos.DrawLine(transform.position, 头);

        Gizmos.DrawLine(头, 头 + new Vector3(0, 头长, -头长));
        Gizmos.DrawLine(头, 头 + new Vector3(0, -头长, -头长));
        Gizmos.DrawLine(头, 头 + new Vector3(头长, 0, -头长));
        Gizmos.DrawLine(头, 头 + new Vector3(-头长, 0, -头长));
    }
    public void 装载(GameObject 挂载物体) {
        挂载物体.SetParent(gameObject);
    }
    public void 放开挂载() {
        挂载物体.transform.SetParent(null);
        挂载物体.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        挂载物体.GetComponent<Rigidbody>().velocity = 父物体.velocity;
        挂载物体 = null;
        //挂载物体.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        //挂载物体.GetComponent<Rigidbody>().freezeRotation = false;
    }
}
