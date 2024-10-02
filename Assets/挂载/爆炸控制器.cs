using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static 战雷革命.公共空间;
using static TGZG.公共空间;
using TGZG;

public class 爆炸控制器 : MonoBehaviour {
    public float 爆炸范围;
    public float 爆炸力度;
    public float 爆炸延迟;

    public Action 爆炸前;
    void Start() {

    }
    void Update() {

    }
    public void 爆炸(Action<Collider> 爆炸物体操作 = null) {
        爆炸前?.Invoke();
        var 爆炸特效 = Instantiate(加载资源<GameObject>("Assets/挂载/爆炸预制体.prefab"));
        爆炸特效.transform.position = transform.position;
        Collider[] colliders = Physics.OverlapSphere(transform.position, 爆炸范围);
        foreach (Collider collider in colliders) {
            Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
            if (rigidbody != null) {
                rigidbody.AddExplosionForce(爆炸力度 * 1000 * 100, transform.position, 爆炸范围);
                爆炸物体操作?.Invoke(collider);
            }
        }
        Destroy(gameObject);
    }
}
