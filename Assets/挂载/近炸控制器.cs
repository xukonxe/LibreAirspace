using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class 近炸控制器 : MonoBehaviour {
    public bool 启动近炸 = false;
    public float 半径 = 10;
    public List<Collider> 近炸排除碰撞箱 = new List<Collider>();
    [HideInInspector]
    public bool 已启动 = false;
    [HideInInspector]
    public 爆炸控制器 爆炸控制器;
    void Start() {
        爆炸控制器 = GetComponent<爆炸控制器>();
    }
    void Update() {
        if (启动近炸 && 已启动) {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 半径);
            var 附近物体 = colliders.ToList();
            附近物体.RemoveAll(collider => 近炸排除碰撞箱.Contains(collider));
            if (附近物体.Count > 0) 爆炸控制器.爆炸();
        }
    }
    public void 启动(float 半径 = default) {
        if (半径 != default) this.半径 = 半径;
        已启动 = true;
    }
}
