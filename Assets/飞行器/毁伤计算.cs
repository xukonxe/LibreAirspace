using System;
using System.Collections;
using System.Collections.Generic;
using TGZG;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;

public class 毁伤计算 : MonoBehaviour {
    public 断裂控制器 断裂控制器;
    public float 动量承载值 = 100000;
    public float 当前承受 = 0;

    public Collider 左翼外沿碰撞体;
    public Collider 右翼外沿碰撞体;
    public Collider 左翼内沿碰撞体;
    public Collider 右翼内沿碰撞体;
    public Collider 左尾翼碰撞体;
    public Collider 右尾翼碰撞体;
    public Collider 垂尾碰撞体;
    public Collider 身体碰撞体;

    public Dictionary<Collider, float> 碰撞体动量 = new Dictionary<Collider, float>();
    public float 左翼外沿碰撞体承载值 = 10000;
    public float 右翼外沿碰撞体承载值 = 10000;
    public float 左翼内沿碰撞体承载值 = 10000;
    public float 右翼内沿碰撞体承载值 = 10000;
    public float 左尾翼碰撞体承载值 = 10000;
    public float 右尾翼碰撞体承载值 = 10000;
    public float 垂尾碰撞体承载值 = 10000;

    public Dictionary<部位, Func<float, bool>> 碰撞控制 = new();
    public Action<string, string, 部位, float, bool> 被击中;
    [NonSerialized]
    public bool 自动损坏 = true;

    void Start() {
        碰撞体动量[左翼外沿碰撞体] = 0;
        碰撞体动量[右翼外沿碰撞体] = 0;
        碰撞体动量[左翼内沿碰撞体] = 0;
        碰撞体动量[右翼内沿碰撞体] = 0;
        碰撞体动量[左尾翼碰撞体] = 0;
        碰撞体动量[右尾翼碰撞体] = 0;
        碰撞体动量[垂尾碰撞体] = 0;

        碰撞控制[部位.右内] = (强度) => {
            碰撞体动量[右翼内沿碰撞体] += 强度;
            if (碰撞体动量[右翼内沿碰撞体] > 右翼内沿碰撞体承载值) {
                断裂控制器.断裂(部位.右内);
                return true;
            }
            return false;
        };
        碰撞控制[部位.左内] = (强度) => {
            碰撞体动量[左翼内沿碰撞体] += 强度;
            if (碰撞体动量[左翼内沿碰撞体] > 左翼内沿碰撞体承载值) {
                断裂控制器.断裂(部位.左内);
                return true;
            }
            return false;
        };
        碰撞控制[部位.右外] = (强度) => {
            碰撞体动量[右翼外沿碰撞体] += 强度;
            if (碰撞体动量[右翼外沿碰撞体] > 右翼外沿碰撞体承载值) {
                断裂控制器.断裂(部位.右外);
                return true;
            }
            return false;
        };
        碰撞控制[部位.左外] = (强度) => {
            碰撞体动量[左翼外沿碰撞体] += 强度;
            if (碰撞体动量[左翼外沿碰撞体] > 左翼外沿碰撞体承载值) {
                断裂控制器.断裂(部位.左外);
                return true;
            }
            return false;
        };
        碰撞控制[部位.左尾] = (强度) => {
            碰撞体动量[左尾翼碰撞体] += 强度;
            if (碰撞体动量[左尾翼碰撞体] > 左尾翼碰撞体承载值) {
                断裂控制器.断裂(部位.左尾);
                return true;
            }
            return false;
        };
        碰撞控制[部位.右尾] = (强度) => {
            碰撞体动量[右尾翼碰撞体] += 强度;
            if (碰撞体动量[右尾翼碰撞体] > 右尾翼碰撞体承载值) {
                断裂控制器.断裂(部位.右尾);
                return true;
            }
            return false;
        };
        碰撞控制[部位.垂] = (强度) => {
            碰撞体动量[垂尾碰撞体] += 强度;
            if (碰撞体动量[垂尾碰撞体] > 垂尾碰撞体承载值) {
                断裂控制器.断裂(部位.垂);
                return true;
            }
            return false;
        };
        碰撞控制[部位.身] = (强度) => {
            当前承受 += 强度;
            if (当前承受 > 动量承载值) {
                //全部断裂
                断裂控制器.断裂(部位.身);
                return true;
            }
            return false;
        };
    }
    void OnCollisionEnter(Collision collision) {
        foreach (ContactPoint contact in collision.contacts) {
            部位 部位 = 部位.无;
            float 强度 = collision.impulse.magnitude;

            Collider hitCollider = contact.thisCollider;
            if (hitCollider == 身体碰撞体) 部位 = 部位.身;
            if (hitCollider == 左翼外沿碰撞体) 部位 = 部位.左外;
            if (hitCollider == 右翼外沿碰撞体) 部位 = 部位.右外;
            if (hitCollider == 左翼内沿碰撞体) 部位 = 部位.左内;
            if (hitCollider == 右翼内沿碰撞体) 部位 = 部位.右内;
            if (hitCollider == 左尾翼碰撞体) 部位 = 部位.左尾;
            if (hitCollider == 右尾翼碰撞体) 部位 = 部位.右尾;
            if (hitCollider == 垂尾碰撞体) 部位 = 部位.垂;

            var 碰撞体 = collision.collider.gameObject;
            var 子弹体 = collision.collider.gameObject.GetComponent<子弹>();
            if (子弹体 != null) {
                //被子弹击中，激活被击中事件，返回攻击者名称
                //被子弹击中，由服务器决定是否损坏，因此损坏参数一直为false
                被击中?.Invoke(子弹体.来源.父飞机.name, gameObject.name, 部位, 强度, false);
            } else {
                //被其他物体击中，激活碰撞控制事件
                if (自动损坏) {//其他玩家不自动损坏，由服务器控制
                    伤害(部位, 强度);
                }
            }
        }
    }
    public HashSet<部位> 损坏数据 => 断裂控制器.断裂状态;
    public void 损坏(HashSet<部位> 数据) {
        foreach (var 部位 in 数据) {
            伤害(部位, 1000000);
        }
    }
    public void 伤害(击伤信息 击伤信息) {
        if (!碰撞控制.ContainsKey(击伤信息.部位)) return;
        if (断裂控制器.断裂状态.Contains(击伤信息.部位)) return;
        var 损坏 = 碰撞控制[击伤信息.部位].Invoke(击伤信息.伤害);
        被击中?.Invoke(击伤信息.攻击者, gameObject.name, 击伤信息.部位, 击伤信息.伤害, 损坏);
    }
    public void 伤害(部位 部位, float 强度) {
        伤害(new 击伤信息 { 部位 = 部位, 伤害 = 强度, 攻击者 = "" });
    }
}
