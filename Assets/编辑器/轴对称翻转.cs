using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
public class 轴对称翻转 : Editor {
    [MenuItem("CONTEXT/Transform/X轴翻转")]
    static void X轴翻转(MenuCommand menuCommand) {
        Transform transform = (Transform)menuCommand.context;
        执行(transform, Vector3.right);
    }

    [MenuItem("CONTEXT/Transform/垂直翻转")]
    static void 垂直翻转(MenuCommand menuCommand) {
        Transform transform = (Transform)menuCommand.context;
        执行(transform, Vector3.up);
    }

    [MenuItem("CONTEXT/Transform/Z轴翻转")]
    static void Z轴翻转(MenuCommand menuCommand) {
        Transform transform = (Transform)menuCommand.context;
        执行(transform, Vector3.forward);
    }

    private static void 执行(Transform transform, Vector3 翻转乘数) {
        Vector3 position = transform.position;
        position = Vector3.Scale(position, 翻转乘数);//scale函数用于将两个向量的对应元素相乘，得到新的向量
        transform.position = position;
    }
}
#endif