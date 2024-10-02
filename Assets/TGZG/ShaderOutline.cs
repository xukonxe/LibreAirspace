using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderOutLine : MonoBehaviour {
    public Color Color;
    public float 边框宽度;
    public float 模糊度;
    public Vector2 尺寸;
    public bool Mix;
    public void Start() {

    }
    //当焦点变化时刷新，避免Unity bug
    public void OnApplicationFocus(bool focus) {
        //Active();
    }
    public void Active() {
        //if (GetComponent<Image>() == null) {
        //    throw new System.Exception("模糊赋值错误！此物体上必须存在Image组件。");
        //}
        //GetComponent<Image>().material = new Material(Shader.Find("Custom/模糊与描边3"));
        //GetComponent<Image>().material.SetColor("BorderC", Color);
        //GetComponent<Image>().material.SetColor("SelfColor", gameObject.GetComponent<Image>().color);
        //GetComponent<Image>().material.SetFloat("Size", 模糊度);
        //GetComponent<Image>().material.SetFloat("BorderW", 边框宽度);
        //GetComponent<Image>().material.SetInt("Mix", Mix ? 1 : 0);
        //GetComponent<Image>().material.SetVector("Rect", 尺寸 = new Vector2(GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height));
        //if (GetComponent<Image>().sprite != null) {
        //    //GetComponent<Image>().material.SetTexture("_MainTex", GetComponent<Image>().sprite.texture);
        //}
    }
    //当尺寸变化时刷新，重新计算边框
    public void OnRectTransformDimensionsChange() {
        GetComponent<Image>().material.SetVector("Rect", 尺寸 = new Vector2(GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height));
    }
}
