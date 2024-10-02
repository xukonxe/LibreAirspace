using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TGZG.公共空间;

using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace TGZG {
    public static partial class 公共空间 {
        public static T GetOrAddComponent<T>(this GameObject X) where T : Component => (X.GetComponent<T>() == null) ? X.AddComponent<T>() : X.GetComponent<T>();
        public static void Destroy(this Object X, bool 立即 = false) {
            if (立即) {
                Object.DestroyImmediate(X);
            } else {
                Object.Destroy(X);
            }
        }
        public static GameObject SetColor(this GameObject X, int A, int B, int C, float D = 1) {
            return X.SetColor(new Vector4(A, B, C, D));
        }
        public static GameObject SetColor(this GameObject X, Vector4 Y) {
            X.GetOrAddComponent<Image>().color = Y.ToColor();
            return X;
        }
        public static Color ToColor(this Vector4 X) {
            return new Color(X.x / 255f, X.y / 255f, X.z / 255f, X.w);
        }
        public static GameObject Find(this GameObject X, string name) {
            return X.Find(t => t.name == name);
        }
        public static GameObject Find(this GameObject X, System.Func<GameObject, bool> func) {
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
        public static List<GameObject> FindAll(this GameObject X, System.Func<GameObject, bool> func) {
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
        public static void OnTextChange(this GameObject X, System.Action<string> act) {
            X.GetComponent<TMP_InputField>().onEndEdit.AddListener(t => act.Invoke(t));
        }
        public static void DeleteOnTextChange(this GameObject X) {
            X.GetComponent<TMP_InputField>().onEndEdit.RemoveAllListeners();
        }
        public static void Clear(this GameObject X, System.Func<GameObject, bool> 条件) {
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
        public static Sprite ToSprite(this Texture2D 纹理) {
            return Sprite.Create(纹理, new Rect(0, 0, 纹理.width, 纹理.height), new Vector2(0.5f, 0.5f));
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
    public static partial class 公共空间 {
        //单击事件
        //public static GameObject OnClick(this GameObject X, System.Action<GameObject> Y) {
        //    X.GetOrAddComponent<UIEvent>().onClick += Y;
        //    return X;//因为是最常用的功能，所以允许返回自身以用于链式调用。其他事件必须单开一行。
        //}
        public static void InvokeOnClick(this GameObject X) => X.GetComponent<UIEvent>()?.onClick?.Invoke(X);
        public static void OnMouseDown(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onMouseDown += Y;
        public static void InvokeOnMouseDown(this GameObject X) => X.GetComponent<UIEvent>()?.onMouseDown?.Invoke(X);
        public static void OnMouseUp(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onMouseUp += Y;
        public static void InvokeOnMouseUp(this GameObject X) => X.GetComponent<UIEvent>()?.onMouseUp?.Invoke(X);
        public static void OnDoubleClick(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onDoubleClick += Y;
        public static void OnRightClick(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onRightClick += Y;
        public static void InvokeOnRightClick(this GameObject X) => X.GetComponent<UIEvent>()?.onRightClick?.Invoke(X);
        //拖动事件
        public static void InvokeOnDrag(this GameObject X) => X.GetComponent<UIEvent>()?.onDrag?.Invoke(X);
        public static void InvokeOnBeginDrag(this GameObject X) => X.GetComponent<UIEvent>()?.onBeginDrag?.Invoke(X);
        public static void OnBeginDrag(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onBeginDrag += Y;
        public static void OnDrag(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onDrag += Y;
        public static void OnEndDrag(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onEndDrag += Y;
        public static void OnDrop(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onDrop += Y;
        //鼠标事件
        public static void OnEnter(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onEnter += Y;
        public static System.Action<GameObject> GetOnEnter(this GameObject X) => X.GetOrAddComponent<UIEvent>().onEnter;
        public static void InvokeOnEnter(this GameObject X) => X.GetComponent<UIEvent>()?.onEnter?.Invoke(X);
        public static void OnMove(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onMove += Y;
        public static void OnExit(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onExit += Y;
        public static System.Action<GameObject> GetOnExit(this GameObject X) => X.GetOrAddComponent<UIEvent>().onExit;
        public static void InvokeOnExit(this GameObject X) => X.GetComponent<UIEvent>()?.onExit?.Invoke(X);
        public static void OnMouseBeginNear(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<MouseNear>().OnBeginNear += Y;
        public static void OnMouseNear(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<MouseNear>().OnNear += Y;
        public static void OnMouseEndNear(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<MouseNear>().OnEndNear += Y;
        public static void OnScroll(this GameObject X, System.Action<GameObject, PointerEventData> Y) => X.GetOrAddComponent<UIEvent>().onScroll += Y;
        //物体生命周期
        public static void OnNextFrame(this GameObject X, System.Action Y) => X.GetOrAddComponent<UIEvent>().nextFrameDo += Y;
        public static void OnUpdate(this GameObject X, System.Action Y) => X.GetOrAddComponent<UIEvent>().updateDo += Y;
        public static void OnSecond(this GameObject X, System.Action Y) => X.GetOrAddComponent<UIEvent>().secondDo += Y;
        public static void DeleteOnSecond(this GameObject X, System.Action Y) => X.GetOrAddComponent<UIEvent>().secondDo -= Y;
        public static void RemoveOnUpdate(this GameObject X, System.Action Y) => X.GetOrAddComponent<UIEvent>().updateDo -= Y;
        public static void OnFixedUpdate(this GameObject X, System.Action Y) => X.GetOrAddComponent<UIEvent>().fixedUpdateDo += Y;
        public static void OnLateUpdate(this GameObject X, System.Action Y) => X.GetOrAddComponent<UIEvent>().lateUpdateDo += Y;
        public static void OnFocusIn(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onSelect += Y;
        public static void OnFocusOut(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onDeselect += Y;
        public static void OnDestory(this GameObject X, System.Action<GameObject> Y) => X.GetOrAddComponent<UIEvent>().onDestory += Y;
        //其他组件
        public static void OnSubmit(this GameObject X, UnityAction<string> Y) => X.GetComponent<TMP_InputField>().onEndEdit.AddListener(Y);
        //public static void OnTextChange(this GameObject X, System.Action<GameObject, string> Y) {
        //    X.GetComponent<TMP_InputField>().onValueChanged.AddListener(t => Y(X, t));
        //}
        //public static void DeleteOnTextChange(this GameObject X) {
        //    X.GetComponent<TMP_InputField>()?.onValueChanged.RemoveAllListeners();
        //}
        //删除
        public static void DeleteOnClick(this GameObject X, System.Action<GameObject> Y = null) {
            if (Y == null) X.GetOrAddComponent<UIEvent>().onClick = null;
            else X.GetOrAddComponent<UIEvent>().onClick -= Y;
        }
        public static void DeleteOnMouseDown(this GameObject X, System.Action<GameObject> Y = null) {
            if (Y == null) X.GetOrAddComponent<UIEvent>().onMouseDown = null;
            else X.GetOrAddComponent<UIEvent>().onMouseDown -= Y;
        }
        public static void DeleteOnMouseUp(this GameObject X, System.Action<GameObject> Y = null) {
            if (Y == null) X.GetOrAddComponent<UIEvent>().onMouseUp = null;
            else X.GetOrAddComponent<UIEvent>().onMouseUp -= Y;
        }
        public static void DeleteOnDoubleClick(this GameObject X, System.Action<GameObject> Y = null) {
            if (Y == null) X.GetOrAddComponent<UIEvent>().onDoubleClick = null;
            else X.GetOrAddComponent<UIEvent>().onDoubleClick -= Y;
        }
        public static void DeleteOnRightClick(this GameObject X, System.Action<GameObject> Y = null) {
            if (Y == null) X.GetOrAddComponent<UIEvent>().onRightClick = null;
            else X.GetOrAddComponent<UIEvent>().onRightClick -= Y;
        }
        //拖动事件
        public static void DeleteOnBeginDrag(this GameObject X, System.Action<GameObject> Y = null) {
            if (Y == null) X.GetOrAddComponent<UIEvent>().onBeginDrag = null;
            else X.GetOrAddComponent<UIEvent>().onBeginDrag -= Y;
        }
        public static void DeleteOnDrag(this GameObject X, System.Action<GameObject> Y = null) {
            if (Y == null) X.GetOrAddComponent<UIEvent>().onDrag = null;
            else X.GetOrAddComponent<UIEvent>().onDrag -= Y;
        }
        public static void DeleteOnEndDrag(this GameObject X, System.Action<GameObject> Y = null) {
            if (Y == null) X.GetOrAddComponent<UIEvent>().onEndDrag = null;
            else X.GetOrAddComponent<UIEvent>().onEndDrag -= Y;
        }
        public static void DeleteOnDrop(this GameObject X, System.Action<GameObject> Y = null) {
            if (Y == null) X.GetOrAddComponent<UIEvent>().onDrop = null;
            else X.GetOrAddComponent<UIEvent>().onDrop -= Y;
        }
        //鼠标事件    
        public static void DeleteOnEnter(this GameObject X, System.Action<GameObject> Y = null) {
            if (Y == null) X.GetOrAddComponent<UIEvent>().onEnter = null;
            else X.GetOrAddComponent<UIEvent>().onEnter -= Y;
        }
        public static void DeleteOnMove(this GameObject X, System.Action<GameObject> Y = null) {
            if (Y == null) X.GetOrAddComponent<UIEvent>().onMove = null;
            else X.GetOrAddComponent<UIEvent>().onMove -= Y;
        }
        public static void DeleteOnExit(this GameObject X, System.Action<GameObject> Y = null) {
            if (Y == null) X.GetOrAddComponent<UIEvent>().onExit = null;
            else X.GetOrAddComponent<UIEvent>().onExit -= Y;
        }
        public static bool IsPanelAtTop(this GameObject X) {
            //检查一个面板与鼠标之间是否存在其他面板
            //思路：发射一个射线，检查碰到的第一个物体是否是此面板
            var A = X.GetComponent<RectTransform>();
            var B = A.四角坐标();
            Vector2[] 四角坐标 = new Vector2[4];
            for (int i = 0; i < 4; i++) {
                四角坐标[i] = RectTransformUtility.WorldToScreenPoint(null, B[i]);
            }
            for (int i = 0; i < 4; i++) {
                var D = Physics2D.Raycast(四角坐标[i], 四角坐标[(i + 1) % 4] - 四角坐标[i], Vector2.Distance(四角坐标[i], 四角坐标[(i + 1) % 4]));
                //Tobo：D.collider永远为空
                D.collider.log();
                if (D.collider != null) {
                    if (D.collider.gameObject == X) {
                        "当前物体在最上层".Log();
                        return true;
                    } else {
                        "当前物体不在最上层".Log();
                        return false;
                    }
                }
            }
            return false;
        }
        public static GameObject SetScrollItem(this GameObject X) { //用于解决滚动阻挡的bug
            X.GetOrAddComponent<UIEvent>().SetScroll();
            return X;
        }
    }
    public static partial class 公共空间 {
        public static GameObject 允许拖动(this GameObject X, GameObject Y = null) {
            if (Y == null) Y = X;
            Vector2 A = Vector2.zero;
            X.OnBeginDrag(t => {
                A = Input.mousePosition - Y.transform.position;
            });
            X.OnDrag(t => {
                Y.transform.position = (Vector2)Input.mousePosition - A;
            });
            return X;
        }
        public static GameObject 允许在其内拖动(this GameObject X, GameObject Y) {
            Vector2 A = Vector2.zero;
            X.OnBeginDrag(t => {
                A = Input.mousePosition - X.transform.position;
            });
            X.OnDrag(t => {
                var 缓存 = X.transform.position;
                X.transform.position = (Vector2)Input.mousePosition - A;
                if (!X.Is在其中(Y)) { //如果物体超出了父物体的范围，将其限制在父物体的范围内
                    X.transform.position = 缓存;
                }
            });
            return X;
        }
        public static bool Is在其中(this GameObject 子, GameObject 父) {
            var 父四角 = 父.GetComponent<RectTransform>().四角坐标();
            var 子四角 = 子.GetComponent<RectTransform>().四角坐标();
            float[] 父边 = new float[4] { 父四角[0].x, 父四角[1].y, 父四角[2].x, 父四角[3].y };
            float[] 子边 = new float[4] { 子四角[0].x, 子四角[1].y, 子四角[2].x, 子四角[3].y };
            return 父边[0] <= 子边[0] && 父边[1] >= 子边[1] && 父边[2] >= 子边[2] && 父边[3] <= 子边[3];
        } /// <summary>
          /// 左下，左上，右上，右下
          /// </summary>
        public static Vector3[] 四角坐标(this RectTransform 矩形) {
            Vector3[] A = new Vector3[4];
            矩形.GetWorldCorners(A);
            return A;
        }
    }
    public class MouseNear : MonoBehaviour {
        public const float 边缘距离 = 10f;
        public bool 上一帧在边缘 = false;
        public System.Action<GameObject> OnBeginNear;
        public System.Action<GameObject> OnNear;
        public System.Action<GameObject> OnEndNear;
        public void Update() {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, null, out Vector2 A);
            var B = IsNearEdge(A);
            if (B) {
                OnNear(gameObject);
                if (!上一帧在边缘) {
                    OnBeginNear(gameObject);
                }
            } else if (上一帧在边缘) {
                OnEndNear(gameObject);
            }
            上一帧在边缘 = B;
        }
        bool IsNearEdge(Vector2 X) {
            if (Input.GetMouseButton(0)) return false;
            if (!gameObject.IsPanelAtTop()) return false;
            return Mathf.Abs(X.x) > GetComponent<RectTransform>().rect.width / 2 - 边缘距离 || Mathf.Abs(X.y) > GetComponent<RectTransform>().rect.height / 2 - 边缘距离;
        }
    }
    public class UIEvent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
    IPointerClickHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler,
    IScrollHandler, ISelectHandler, IUpdateSelectedHandler, IDeselectHandler {
        public System.Action<GameObject> onClick;
        public System.Action<GameObject> onDoubleClick;
        public System.Action<GameObject> onRightClick;
        public System.Action<GameObject> onMouseDown;
        public System.Action<GameObject> onMouseUp;
        public System.Action<GameObject> onEnter;
        public System.Action<GameObject> onExit;
        public System.Action<GameObject> onSelect;
        public System.Action<GameObject> onUpdateSelected;
        public System.Action<GameObject> onDeselect;
        public System.Action<GameObject> onMove;
        public System.Action<GameObject> onBeginDrag;
        public System.Action<GameObject> onDrag;
        public System.Action<GameObject> onDrop;
        public System.Action<GameObject> onEndDrag;
        public System.Action<GameObject> onDestory;
        public System.Action<GameObject, PointerEventData> onScroll;
        public System.Action updateDo;
        public System.Action nextFrameDo;
        public System.Action fixedUpdateDo;
        public System.Action lateUpdateDo;
        public System.Action secondDo;
        public void Awake() {
            if (GetComponent<Image>() == null && GetComponent<TextMeshProUGUI>() == null) gameObject.SetColor(0, 0, 0, 0);//添加一个透明图片，从而能够遮挡鼠标事件
        }
        private float 当前时间;
        public void Update() {
            updateDo?.Invoke();
            当前时间 += Time.deltaTime;
            if (当前时间 >= 1) {
                当前时间 = 0;
                secondDo?.Invoke();
            }
        }
        public void FixedUpdate() => fixedUpdateDo?.Invoke();
        public void LateUpdate() {
            nextFrameDo?.Invoke();
            nextFrameDo = null;
            lateUpdateDo?.Invoke();
        }
        public void OnDestroy() => onDestory?.Invoke(gameObject);
        public void SetScroll() { //避免物体阻挡拖动
            onScroll = (t, e) => {
                var A = GetComponentInParent<ScrollRect>();
                if (A == null) return;
                A.content.anchoredPosition -= new Vector2(0, e.scrollDelta.y * 50f);
            };
        }
        public void OnPointerClick(PointerEventData X) {
            if (X.clickCount == 2) onDoubleClick?.Invoke(gameObject);
            if (X.clickCount == 1) {
                if (X.button == PointerEventData.InputButton.Left) onClick?.Invoke(gameObject);
                if (X.button == PointerEventData.InputButton.Right) onRightClick?.Invoke(gameObject);
            }
        }
        public void OnPointerDown(PointerEventData X) => onMouseDown?.Invoke(gameObject);
        public void OnPointerUp(PointerEventData X) => onMouseUp?.Invoke(gameObject);
        public void OnPointerEnter(PointerEventData X) => onEnter?.Invoke(gameObject);
        public void OnPointerExit(PointerEventData X) => onExit?.Invoke(gameObject);
        public void OnPointerMove(PointerEventData X) => onMove?.Invoke(gameObject);
        public void OnScroll(PointerEventData X) => onScroll?.Invoke(gameObject, X);
        public void OnSelect(BaseEventData X) => onSelect?.Invoke(gameObject);
        public void OnUpdateSelected(BaseEventData X) => onUpdateSelected?.Invoke(gameObject);
        public void OnDeselect(BaseEventData X) => onDeselect?.Invoke(gameObject);
        public void OnBeginDrag(PointerEventData X) => onBeginDrag?.Invoke(gameObject);
        public void OnDrag(PointerEventData X) => onDrag?.Invoke(gameObject);
        public void OnDrop(PointerEventData X) => onDrop?.Invoke(gameObject);
        public void OnEndDrag(PointerEventData X) => onEndDrag?.Invoke(gameObject);
    }
}
