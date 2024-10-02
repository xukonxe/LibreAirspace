using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 枪射指示器 : MonoBehaviour {
    public GameObject 父飞机;
    public GameObject 子弹模板;
    public GameObject 子弹模板1;
    public GameObject 子弹模板2;
    public GameObject 子弹模板3;
    public float 每分射速;
    public float 子弹重量;
    public float 子弹速度;

    float 长 = 5f; // 箭头大小
    float 头长 = 0.2f; // 箭头大小

    void Start() {

    }
    void Update() {

    }
    void OnDrawGizmosSelected() {
        // 计算箭头的方向
        Vector3 方向 = transform.forward;
        Vector3 头 = transform.position + 方向 * 长;

        // 绘制箭头
        Gizmos.color = Color.red; // 设置箭头颜色
        Gizmos.DrawLine(transform.position, 头);

        Gizmos.DrawLine(头, 头 + new Vector3(0, 头长, -头长));
        Gizmos.DrawLine(头, 头 + new Vector3(0, -头长, -头长));
        Gizmos.DrawLine(头, 头 + new Vector3(头长, 0, -头长));
        Gizmos.DrawLine(头, 头 + new Vector3(-头长, 0, -头长));
    }
    public void 射击(Transform 依附载具) {
        GameObject 子弹 = Instantiate(子弹模板, transform.position, 依附载具.rotation * transform.rotation);
        Vector3 方向 = gameObject.transform.TransformDirection(Vector3.forward);
        //直接设置速度，忽略质量
        子弹.GetComponent<Rigidbody>().AddForce(方向 * 子弹速度, ForceMode.VelocityChange);
        子弹.GetComponent<Rigidbody>().mass = 子弹重量;
        //设置来源指示
        子弹.GetComponent<子弹>().来源 = this;
    }
}
