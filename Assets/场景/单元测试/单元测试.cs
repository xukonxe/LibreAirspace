using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using static 战雷革命.公共空间;
using static TGZG.公共空间;
using TGZG;
using 战雷革命;

public class 单元测试 : MonoBehaviour {
    void Start() {

        var A = new 玩家游玩数据();
        A.u = new 玩家进入数据();
        A.u.n = "123456";
        A.u.tp = 载具类型.m15n23;
        A.p = new 玩家世界数据();
        A.p.p = new Vector3(0.3235684523f, 0.3654684651f, 0.345646456465f).To向量3();
        A.p.d = new Quaternion(0.3235684523f, 0.3654684651f, 0.345646456465f, 0.345646456465f).To向量4();
        A.p.v = new Vector3(0.3235684523f, 0.3654684651f, 0.345646456465f).To向量3();
        A.p.r = new Vector3(0.3235684523f, 0.3654684651f, 0.345646456465f).To向量3();
       
        A.射 = new int[] { 1, 1 };
        A.msl = new List<导弹飞行数据>() {
            new 导弹飞行数据(){
                编号=1,
                tp=挂载类型.AIM9E,
                v=new Vector3(0.3235684523f, 0.3654684651f, 0.345646456465f).To向量3(),
                d=new Quaternion(0.3235684523f, 0.3654684651f, 0.345646456465f, 0.345646456465f).To向量4(),
                p=new Vector3(0.3235684523f, 0.3654684651f, 0.345646456465f).To向量3(),
                r=new Vector3(0.3235684523f, 0.3654684651f, 0.345646456465f).To向量3(),
            }
        };
        A.三位保留();
        var json = A.ToJson(格式美化: false);
        json.log();
        var kbs = json.GetKbs();

        $"128Tick：{kbs * 128}kb/s 64Tick：{kbs * 64}kb/s 32Tick：{kbs * 32}kb/s".log();


    }
    void Update() {

    }
}
