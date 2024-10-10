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
        public 模式类型 模式;
        public List<载具类型> 可选载具;
        public List<队伍> 可选队伍;
    }
    public enum 模式类型 {
        休闲,
        竞技,
        自定义
    }
    //游戏通讯，每秒发送数十次。为了减低带宽压力，字段名尽和内容要可能短。
    public struct 玩家游玩数据 {
        public 玩家进入数据 u;
        public 玩家世界数据 p;
        public int[] 射;
        public List<导弹飞行数据> msl;
        public HashSet<部位> 损坏;
        public void 三位保留() {
            //所有float只保留三位小数
            p.p = p.p.Select(t => (float)Math.Round(t, 3)).ToArray();
            p.d = p.d.Select(t => (float)Math.Round(t, 3)).ToArray();
            p.v = p.v.Select(t => (float)Math.Round(t, 3)).ToArray();
            p.r = p.r.Select(t => (float)Math.Round(t, 3)).ToArray();
            for (int i = 0; i < msl.Count; i++) {
                var n = new 导弹飞行数据();
                n.编号 = msl[i].编号;
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
        public int 编号;
        public 挂载类型 tp;
        public float[] p;
        public float[] d;
        public float[] v;
        public float[] r;
    }
    public enum 挂载类型 {
        无,
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
        public string 攻击者;
        public string 被攻者;
        public float 伤害;
        public 部位 部位;
    }
    public struct 玩家进入数据 {
        public string n;
        public 载具类型 tp;
        public 队伍 tm;
        public TimeSpan 油量;
        public (string, string) 出生点;
        public 挂载类型[] 挂载;
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
    public struct 计分板数据 {
        public string[] 列定义;
        public List<object[]> 列数据;
        public object[] 取行(int 行号) {
            return 列数据[行号];
        }
        public object[] 取列(string 列名) {
            var 列号 = Array.IndexOf(列定义, 列名);
            if (列号 < 0) return null;
            return 列数据.Select(t => t[列号]).ToArray();
        }
    }
}
