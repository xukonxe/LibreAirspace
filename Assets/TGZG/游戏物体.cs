using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;
using XLua;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System;

namespace TGZG {
    public static partial class 公共空间 {
        public static GameObject Find(this GameObject X, string name) {
            return X.Find(t => t.name == name);
        }
        public static GameObject Find(this GameObject X, Func<GameObject, bool> func) {
            // 如果传入的GameObject为null，则直接返回null
            if (X == null) return null;
            //获取所有子物体
            List<GameObject> childs = new List<GameObject>();
            foreach (Transform child in X.transform) {
                childs.Add(child.gameObject);
            }
            //遍历所有子物体，查找匹配的名称的物体
            foreach (GameObject child in childs) {
                if (func(child)) {
                    return child;
                }
            }
            // 如果遍历完所有子物体都没有找到匹配的名称，则返回null
            return null;
        }
        public static List<GameObject> FindAll(this GameObject X, Func<GameObject, bool> func) {
            // 如果传入的GameObject为null，则直接返回null
            if (X == null) return null;
            //获取所有子物体
            List<GameObject> childs = new List<GameObject>();
            foreach (Transform child in X.transform) {
                childs.Add(child.gameObject);
            }
            //遍历所有子物体，查找匹配的名称的物体
            List<GameObject> result = new List<GameObject>();
            foreach (GameObject child in childs) {
                if (func(child)) {
                    result.Add(child);
                }
            }
            return result;
        }
        public static GameObject FindInChildren(this GameObject X, string name) {
            return X.transform.Find(name).gameObject;
        }
        public static GameObject GetParent(this GameObject X) {
            return X.transform.parent.gameObject;
        }
        public static GameObject Instantiate(this GameObject X, GameObject Parent) {
            GameObject obj = UnityEngine.Object.Instantiate(X);
            obj.transform.SetParent(Parent.transform);
            return obj;
        }
        public static void Destroy(this GameObject X) {
            UnityEngine.Object.Destroy(X);
        }
        public static void Destroy(this MonoBehaviour X) {
            UnityEngine.Object.Destroy(X);
        }
        public static void SetParent(this GameObject X, GameObject Parent) {
            X.transform.SetParent(Parent.transform);
        }
        public static GameObject SetText(this GameObject X, string Y) {
            if (X.GetComponent<TextMeshProUGUI>() != null) {
                X.GetComponent<TextMeshProUGUI>().text = Y;
            } else {
                X.GetComponent<TMP_InputField>().text = Y;
            }
            return X;
        }
        public static string GetText(this GameObject X) {
            if (X.GetComponent<TextMeshProUGUI>() != null) {
                return X.GetComponent<TextMeshProUGUI>().text;
            } else {
                return X.GetComponent<TMP_InputField>().text;
            }
        }
        public static GameObject AddText(this GameObject X, string Y) {
            X.GetComponent<TextMeshProUGUI>().text += "\n" + Y;
            return X;
        }
        public static GameObject OnClick(this GameObject X, System.Action<GameObject> Y) {
            X.GetComponent<Button>().onClick.AddListener(() => {
                Y(X);
            });
            return X;
        }
        public static void DeleteOnClick(this GameObject X) {
            X.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        public static void OnTextChange(this GameObject X, Action<string> act) {
            X.GetComponent<TMP_InputField>().onEndEdit.AddListener(t => act.Invoke(t));
        }
        public static void DeleteOnTextChange(this GameObject X) {
            X.GetComponent<TMP_InputField>().onEndEdit.RemoveAllListeners();
        }
        public static void Clear(this GameObject X, Func<GameObject, bool> 条件) {
            foreach (Transform child in X.transform) {
                if (条件(child.gameObject)) {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }
        }
        public static void Clear(this GameObject X) {
            foreach (Transform child in X.transform) {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }
        public static void SetAsFirst(this GameObject X) {
            X.transform.SetAsFirstSibling();
        }
        public static void SetAsLast(this GameObject X) {
            X.transform.SetAsLastSibling();
        }
        public static T 加载资源<T>(string 路径) {
            var Handle = Addressables.LoadAssetAsync<T>(路径);
            return Handle.WaitForCompletion();
        }
        public static GameObject 映射到世界物体(this GameObject 屏幕物体, GameObject 世界物体, Vector3 世界偏移 = default, Vector3 屏幕偏移 = default) {
            //如果世界物体在摄像机方向上的投影为负数，则不显示
            var 屏幕映射位置 = Camera.main.WorldToScreenPoint(世界物体.transform.position + 世界偏移);
            if (屏幕映射位置.z > 0) {
                屏幕物体.transform.position = new Vector3(屏幕映射位置.x, 屏幕映射位置.y) + 屏幕偏移;//光标是Ugui物体,position是屏幕位置
            } else {
                屏幕物体.transform.position = new(-1000, -1000);
            }
            return 屏幕物体;
        }
    }
}
