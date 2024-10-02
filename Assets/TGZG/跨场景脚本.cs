using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;

using UnityEngine.SceneManagement;

public class 跨场景脚本 : MonoBehaviour {
    // 脚本挂载时，将此物体设置为多场景共用物体。
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }
}
