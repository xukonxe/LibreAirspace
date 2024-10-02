using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 枪射 : MonoBehaviour {
    public 枪射指示器 控制器;
    void Start() {
        控制器 = GetComponent<枪射指示器>();
    }

    // Update is called once per frame
    void Update() {
        if (控制器 != null) {
            if (Input.GetMouseButton(0)) {
                控制器.射击(transform);
            }
        }
    }
}
