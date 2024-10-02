using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using 战雷革命;

public class 悬浮界面UI管理器 : MonoBehaviour {
    public void 初始化(Action<悬浮界面UI管理器> onInit) => onInit(this);
    public Slider 蓝队比分UI;
    public Slider 红队比分UI;
    public TextMeshProUGUI 蓝队分数UI;
    public TextMeshProUGUI 红队分数UI;
    public Func<队伍数据类> DataFrom;

    public 飞行数据显示器 飞行数据显示;
    void Start() {

    }
    void Update() {
        if (DataFrom != null) {
            var data = DataFrom();

            蓝队比分UI.maxValue = data.蓝队满;
            蓝队比分UI.value = data.蓝队分数;
            蓝队分数UI.text = data.蓝队分数.ToString();

            红队比分UI.maxValue = data.红队满;
            红队比分UI.value = data.红队分数;
            红队分数UI.text = data.红队分数.ToString();
        }
    }
    public bool 显示 => gameObject.activeSelf;
    public void 打开() {
        gameObject.SetActive(true);
    }
    public void 关闭() {
        gameObject.SetActive(false);
    }
}
