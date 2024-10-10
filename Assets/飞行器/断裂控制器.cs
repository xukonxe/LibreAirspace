using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 断裂控制器 : MonoBehaviour {

    public GameObject 左翼外沿;
    public GameObject 右翼外沿;
    public GameObject 左翼内沿;
    public GameObject 右翼内沿;
    public GameObject 左尾翼;
    public GameObject 右尾翼;
    public GameObject 垂尾;

    public bool 左外断 => 左翼外沿 == null;
    public bool 右外断 => 右翼外沿 == null;
    public bool 左内断 => 左翼内沿 == null;
    public bool 右内断 => 右翼内沿 == null;
    public bool 左尾断 => 左尾翼 == null;
    public bool 右尾断 => 右尾翼 == null;
    public bool 垂断 => 垂尾 == null;
    public bool 全断 => 左外断 && 右外断 && 左内断 && 右内断 && 左尾断 && 右尾断 && 垂断;

    public bool 尾翼一体 = false;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Keypad1)) {
            左翼外沿断裂();
        }
        if (Input.GetKeyDown(KeyCode.Keypad3)) {
            右翼外沿断裂();
        }
        if (Input.GetKeyDown(KeyCode.Keypad4)) {
            左翼内沿断裂();
        }
        if (Input.GetKeyDown(KeyCode.Keypad6)) {
            右翼内沿断裂();
        }
        if (Input.GetKeyDown(KeyCode.Keypad5)) {
            左尾翼断裂();
        }
        if (Input.GetKeyDown(KeyCode.Keypad7)) {
            右尾翼断裂();
        }
        if (Input.GetKeyDown(KeyCode.Keypad9)) {
            垂尾断裂();
        }
    }
    public void 左翼外沿断裂() {
        断裂(左翼外沿);
    }
    public void 右翼外沿断裂() {
        断裂(右翼外沿);
    }
    public void 左翼内沿断裂() {
        断裂(左翼内沿);
    }
    public void 右翼内沿断裂() {
        断裂(右翼内沿);
    }
    public void 左尾翼断裂() {
        断裂(左尾翼);
    }
    public void 右尾翼断裂() {
        断裂(右尾翼);
    }
    public void 垂尾断裂() {
        断裂(垂尾);
    }
    public void 全断裂() {
        左翼外沿断裂();
        右翼外沿断裂();
        左翼内沿断裂();
        右翼内沿断裂();
        左尾翼断裂();
        右尾翼断裂();
        垂尾断裂();
    }
    void 断裂(GameObject 断裂物) {
        if (断裂物 == null) return;
        var 断裂物克隆 = Instantiate(断裂物, 断裂物.transform.position, 断裂物.transform.rotation);
        Destroy(断裂物);
        断裂物克隆.AddComponent<Rigidbody>();
    }
}

