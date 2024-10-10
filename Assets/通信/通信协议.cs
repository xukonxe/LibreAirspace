using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static 战雷革命.公共空间;
using static TGZG.公共空间;
using TGZG;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace 战雷革命 {
    //房间列表通讯协议
    public struct 所有房间数据类 {
        public List<房间数据类> 房间列表;
        [JsonIgnore]
        public int 房间总数 => 房间列表.Count;
        [JsonIgnore]
        public 房间数据类 this[int index] => 房间列表[index];
        [JsonIgnore]
        public 房间数据类 this[string 房间名] => 房间列表.Find(x => x.房间名 == 房间名);
        public bool 存在房间(string 房间名) => 房间列表.Exists(x => x.房间名 == 房间名);
    }
    public struct 房间数据类 {
        public string IP;
        public string 房间名;
        public string 房间描述;
        public string 房主;
        public int 人数;
        public string 地图名;
        public bool 房间密码;
        public string 房间版本;
        public int 每秒同步次数;
        public DateTime 房间创建时间;
    }
    //游戏通讯，每秒发送数十次。为了减低带宽压力，字段名尽和内容要可能短。
    public struct 玩家游玩数据 {
        public 玩家登录数据 u;
        public 玩家世界数据 p;
        public 队伍 tm;
        public int[] 射;
        public List<导弹飞行数据> msl;
        public void 三位保留() {
            //所有float只保留三位小数
            p.p = p.p.Select(t => (float)Math.Round(t, 3)).ToArray();
            p.d = p.d.Select(t => (float)Math.Round(t, 3)).ToArray();
            p.v = p.v.Select(t => (float)Math.Round(t, 3)).ToArray();
            p.r = p.r.Select(t => (float)Math.Round(t, 3)).ToArray();
            for (int i = 0; i < msl.Count; i++) {
                var n = new 导弹飞行数据();
                n.i = msl[i].i;
                n.tp = msl[i].tp;
                n.p = msl[i].p.Select(t => (float)Math.Round(t, 3)).ToArray();
                n.d = msl[i].d.Select(t => (float)Math.Round(t, 3)).ToArray();
                n.v = msl[i].v.Select(t => (float)Math.Round(t, 3)).ToArray();
                n.r = msl[i].r.Select(t => (float)Math.Round(t, 3)).ToArray();
                msl[i] = n;
            }
        }
    }
    public struct 导弹飞行数据 {
        public int i;
        public 导弹类型 tp;
        public float[] p;
        public float[] d;
        public float[] v;
        public float[] r;
    }
    public enum 导弹类型 {
        AIM9E,
    }
    public enum 部位 {
        无,
        身,
        左外,
        右外,
        左内,
        右内,
        左尾,
        右尾,
        垂,
    }
    public struct 击伤信息 {
        public string ths;
        public float dm;
        public 部位 bp;
    }
    public struct 玩家登录数据 {
        public string n;
        public 载具类型 tp;
    }
    public struct 玩家世界数据 {
        public float[] p;
        public float[] d;
        public float[] v;
        public float[] r;
    }
    public enum 载具类型 {
        无,
        m15n23,
        f86f25,
        f4c,
        m21pfm,
        P51h
    }
    public enum 队伍 {
        无,
        蓝,
        红,
        系统
    }
}
