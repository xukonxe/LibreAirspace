using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 初始速度赋值 : MonoBehaviour {
    public float 初始扔出速度 = 10f;
    void Start() {
        GetComponent<Rigidbody>().velocity = transform.forward * 初始扔出速度;

    }

    // Update is called once per frame
    void Update() {

    }
}
