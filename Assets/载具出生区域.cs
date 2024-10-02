using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;

public class 载具出生区域 : MonoBehaviour {
    public string 出生区名称;
    public 队伍 所属队伍;
    [Range(0, 1000)]
    public float 长 = 10;
    [Range(0, 1000)]
    public float 宽 = 10;
    [Range(0, 1000)]
    public float 高 = 10;
    public List<载具出生点> 出生点列表 = new();
    void Start() {

    }
    void Update() {

    }
    void OnDrawGizmos() {
        if (所属队伍 is 队伍.蓝) {
            Gizmos.color = Color.blue;
        } else if (所属队伍 is 队伍.红) {
            Gizmos.color = Color.red;
        } else {
            Gizmos.color = Color.gray;
        }
        var 本物体旋转 = transform.rotation;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, 本物体旋转, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(宽, 高, 长));
        

       
    }
}
