using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 动能测试 : MonoBehaviour {
    public float 向下速度;
    void Start() {
        GetComponent<Rigidbody>().velocity = new Vector3(0, -向下速度, 0);

    }

    // Update is called once per frame
    void Update() {

    }
    void OnCollisionEnter(Collision collision) {
        print($"动量：{collision.impulse.magnitude}");
    }
}
