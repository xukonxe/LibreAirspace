using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class esc界面UI管理器 : MonoBehaviour {
    public void 初始化(Action<esc界面UI管理器> onInit) => onInit(this);
    public Button 返回主界面按钮;
    public Button 重生按钮;
    public Button 键位按钮;
    public Button 退出游戏按钮;
    void Start() {

    }
    void Update() {

    }
    public bool 显示 => gameObject.activeSelf;
    public void 打开() {
        gameObject.SetActive(true);
    }
    public void 关闭() {
        gameObject.SetActive(false);
    }
}
