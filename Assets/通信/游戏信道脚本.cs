using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static 战雷革命.公共空间;
using static TGZG.公共空间;
using TGZG;
using 战雷革命;
using UnityEngine.Events;
using System;

public class 游戏服务器信道类 : 网络信道类_Kcp {
    public 游戏服务器信道类(string 版本) : base(版本) { }
    public void 发送登录验证(string 名称, string 密码) {

    }
    public void 上传损坏提示(string 攻击者, 部位 损坏数据) {
        Send(
            ("标题", "损坏"),
            ("攻击者", 攻击者),
            ("数据", 损坏数据.ToString())
            );
    }
    public void 击伤(击伤信息 攻击者) {
        Send(
            ("标题", "击伤"),
            ("数据", 攻击者.ToJson())
            );
    }
    public void 导弹爆炸(导弹飞行数据 爆炸数据) {
        Send(
            ("标题", "导弹爆炸"),
            ("数据", 爆炸数据.ToJson()));
    }
    public void 导弹发射(导弹飞行数据 发射数据) {
        Send(
            ("标题", "导弹发射"),
            ("数据", 发射数据.ToJson()));
    }
    public void 发送重生(玩家进入数据 登录数据) {
        Send(
            ("标题", "重生"),
            ("数据", 登录数据.ToJson()));
    }
    public void 发送数据(玩家游玩数据 数据) {
        Send(
            ("标题", "更新位置"),
            ("数据", 数据.ToJson()));
    }
    public void 发送聊天消息(string 名称, string 内容) {
        Send(
            ("标题", "发送聊天消息"),
            ("内容", 内容),
            ("发送者", 名称));
    }
    public void 登录(玩家进入数据 数据) {
        Send(
            ("标题", "登录"),
            ("数据", 数据.ToJson()));
    }
}
