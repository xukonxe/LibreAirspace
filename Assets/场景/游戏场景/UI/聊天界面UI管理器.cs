using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class 聊天界面UI管理器 : MonoBehaviour {
    public void 初始化(Action<聊天界面UI管理器> onInit) => onInit(this);
    public Button 发送按钮;
    public Button 清空按钮;
    public TMP_InputField 输入框;
    public ScrollRect 滚动区域;
    public TextMeshProUGUI 聊天记录;
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
