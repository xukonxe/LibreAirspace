using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static 战雷革命.公共空间;
using static TGZG.公共空间;
using TGZG;
using 战雷革命;
using static UnityEngine.EventSystems.EventTrigger;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.Serialization;
namespace 战雷革命 {
    public class 房间服务器信道类 : 网络信道类_Tcp {
        public 房间服务器信道类(string ip, string 版本) : base(ip, 版本) { }
        public async Task<所有房间数据类> 获取房间列表() {
            var 数据 = await 游戏端.SendAsync(new() {
                { "标题","请求服务器列表"},
            });
            return 数据["房间列表"].JsonToCS<所有房间数据类>();
        }
        public async Task 登录(string 用户名, string 密码, Action 成功, Action<string> 失败) {
            var 返回消息 = await 游戏端.SendAsync(new() {
                { "标题","登录"},
                { "账号",用户名},
                { "密码",密码},
            });
            //处理回调
            if (返回消息.ContainsKey("状态")
                &&
                返回消息["状态"] == "登录成功") 成功();
            if (返回消息.ContainsKey("验证失败")) 失败(返回消息["验证失败"]);
        }
        public async Task 注册(string 用户名, string 密码, Action 成功, Action<string> 失败) {
            var 返回消息 = await 游戏端.SendAsync(new() {
                { "标题","注册"},
                { "账号",用户名},
                { "密码",密码},
            });
            //处理回调
            if (返回消息.ContainsKey("状态")) {
                if (返回消息["状态"] == "注册成功") 成功();
                else 失败(返回消息["状态"]);
            }
            if (返回消息.ContainsKey("验证失败")) 失败(返回消息["验证失败"]);
        }
    }
}
