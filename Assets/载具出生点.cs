using System.Collections;
using System.Collections.Generic;
using TGZG;
using UnityEditor;
using UnityEngine;
public class ReadOnlyAttribute : PropertyAttribute {
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif
public class 载具出生点 : MonoBehaviour {
    public string 出生点名称;
    [ReadOnly]
    public bool 区域内有载具;
    public bool 空出;
    public float 初始速度;

    [Range(0, 100)]
    public float 长 = 10;
    [Range(0, 100)]
    public float 宽 = 10;
    [Range(0, 100)]
    public float 高 = 10;

    void Start() {

    }
    void Update() {

    }
    private void FixedUpdate() {
        //检测区域内是否有载具
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(宽 / 2, 高 / 2, 长 / 2), transform.rotation);
        区域内有载具 = false;
        foreach (Collider collider in colliders) {
            //collider.gameObject.name.log();
            区域内有载具 = true;
            break;
        }
    }
    void OnDrawGizmosSelected() {

        var 本物体旋转 = transform.rotation;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, 本物体旋转, Vector3.one);
        //方框区域指示
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(宽, 高, 长));
        //三角形方向指示
        Vector3[] points = new Vector3[3];
        points[0] = new Vector3(-宽 / 2, -高 / 2, -长 / 2);
        points[1] = new Vector3(宽 / 2, -高 / 2, -长 / 2);
        points[2] = new Vector3(0, -高 / 2, 0);
        Gizmos.DrawLine(points[0], points[1]);
        Gizmos.DrawLine(points[1], points[2]);
        Gizmos.DrawLine(points[2], points[0]);
    }
}
