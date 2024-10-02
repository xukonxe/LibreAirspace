using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;
using UnityEngine.UI;

public class 测试场景管理器 : MonoBehaviour {
    public Image 图片;
    public 小地图管理器 测试小地图;
    public Transform 载具;
    Vector2 随机位置 => new Vector2(Random.Range(0f, 10000f), Random.Range(0f, 10000f));
    void Start() {
        测试小地图.DataFrom = () => new List<玩家小地图信息> {
            new 玩家小地图信息 { 主玩家 = true ,位置 = new(载具.position.x, 载具.position.z), 旋转 = -载具.rotation.eulerAngles.y , 队伍 = 队伍.红 },
            new 玩家小地图信息 { 位置 = 随机位置, 队伍=队伍.蓝 },
        };
    }
    void Update() {

    }
}
