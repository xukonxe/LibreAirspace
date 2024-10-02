using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;

public class 地图管理器 : MonoBehaviour {
    public int 宽度 => (int)gameObject.Find("Terrain").GetComponent<Terrain>().terrainData.size.x;
    public List<载具出生区域> 出生区域列表;
    public 玩家世界数据 get出生数据(玩家进入数据 数据) {

        Vector3 出生位置 = new Vector3(1000, 1000, 1000);
        Quaternion 出生方向 = Quaternion.identity;

        var 出生区域 = 出生区域列表.Find(x => x.出生区名称 == 数据.出生点.Item1);
        if (出生区域 == null) {
            Debug.LogError("没有找到名称为" + 数据.出生点.Item1 + "的出生区域");
        }
        var 出生点 = 出生区域.出生点列表.Find(x => x.出生点名称 == 数据.出生点.Item2);
        if (出生点 == null) {
            Debug.LogError("没有找到名称为" + 数据.出生点.Item2 + "的出生点");
        }
        var 出生点位置 = 出生点.transform.position;
        var 出生点方向 = 出生点.transform.rotation;
        var 世界位置 = 出生区域.transform.TransformPoint(出生点位置);
        var 世界方向 = 出生区域.transform.rotation;

        出生位置 = 出生点位置;
        出生方向 = 出生点方向;
        return new 玩家世界数据() {
            p = 出生位置.To向量3(),
            d = 出生方向.To向量4(),
            r = Vector3.zero.To向量3(),
            v = Vector3.zero.To向量3(),
        };
    }
    void Start() {

    }
    void Update() {

    }
}
