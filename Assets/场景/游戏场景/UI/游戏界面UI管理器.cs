using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class 游戏界面UI管理器 : MonoBehaviour {
    public void 初始化(Action<游戏界面UI管理器> onInit) => onInit(this);
    public GameObject 导弹外环;
    public GameObject 导弹内环;

    public TextMeshProUGUI 摧毁空中目标提示;
    public TextMeshProUGUI 目标名称显示;

    int 维持秒数;
    int 消失秒数;
    Stopwatch 计时器;
    Action 消失每帧;
    void Start() {

    }
    void Update() {
        消失每帧?.Invoke();
    }
    public bool 显示 => gameObject.activeSelf;
    public void 打开() {
        gameObject.SetActive(true);
    }
    public void 关闭() {
        gameObject.SetActive(false);
    }
    public void 提示摧毁空中目标(string name) {
        摧毁空中目标提示.gameObject.SetActive(true);
        目标名称显示.gameObject.SetActive(true);
        目标名称显示.text = $"{name} 已摧毁";
        逐渐消失(3, 2);
    }
    public void 逐渐消失(int 维持秒数, int 消失秒数) {
        计时器 = Stopwatch.StartNew();
        this.维持秒数 = 维持秒数;
        this.消失秒数 = 消失秒数;
        消失每帧 = () => {
            if (计时器.Elapsed.Seconds >= 维持秒数) {
                var color = 目标名称显示.color;
                color.a -= ((float)1) / 消失秒数 * Time.deltaTime;
                目标名称显示.color = color;

                var color2 = 摧毁空中目标提示.color;
                color2.a -= ((float)1) / 消失秒数 * Time.deltaTime;
                摧毁空中目标提示.color = color2;
            }
            if (计时器.Elapsed.Seconds >= 维持秒数 + 消失秒数) {
                摧毁空中目标提示.gameObject.SetActive(false);
                目标名称显示.gameObject.SetActive(false);
                消失每帧 = null;
                计时器.Stop();
                计时器 = null;
            }
        };
    }
}
