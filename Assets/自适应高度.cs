using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TGZG;
using UnityEngine;
using 战雷革命;
using PID;

public class 自适应高度 : MonoBehaviour {
    public LineRenderer line1;
    public LineRenderer line2;
    public LineRenderer line3;
    public LineRenderer line4;

    public float 目标高度 => 目标高度物体.position.y;
    public Transform 目标高度物体;

    public float 最大力量 = 10;
    [Range(-10, 10)]
    public float debug线位置 = 2;
    自适应修正控制器 高度控制器;
    [Header("PID参数")]
    public float 差值修正系数;
    public float 偏移修正速率;
    public int 自适应远离中心抑制步进帧数间隔;
    public float 抑制步进步进;
    public float 回正步进步进;
    public float 最大抑制修正步进 = 0.1f;
    public float 最大抖动修正量 = 0.2f;

    public bool 使用差值修正 = true;
    public bool 使用偏移修正 = true;
    public bool 使用自适应抖动修正 = true;


    [Header("输出参数")]
    [Range(-1, 1)]
    public float 差值修正输出;
    [Range(-1, 1)]
    public float 偏移修正输出;
    [Range(-1, 1)]
    public float 抑制步进;
    [Range(-1, 1)]
    public float 自适应抖动修正输出;
    [Range(-1, 1)]
    public float 输出;

    [Range(0.01f,2f)]
    public float 时间流速 = 1f;
    void Start() {
        高度控制器 = new();
        var 参数 = new 控制器参数();
        参数.差值修正系数 = 差值修正系数;
        参数.偏移修正速率 = 偏移修正速率;
        参数.自适应远离中心抑制步进帧数间隔 = 自适应远离中心抑制步进帧数间隔;
        参数.抑制步进步进 = 抑制步进步进;
        参数.回正步进步进 = 回正步进步进;
        参数.最大抑制修正步进 = 最大抑制修正步进;
        参数.最大抖动修正量 = 最大抖动修正量;
        高度控制器.参数 = 参数;
        高度控制器.实际值控制方法 += t => {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 最大力量 * (float)t);
        };
    }
    void Update() {
        //设置时间流速
        Time.timeScale = 时间流速;

        line1.startWidth = 0.1f;
        line1.endWidth = 0.1f;
        line1.positionCount = 2;
        line1.SetPosition(0, new Vector3(transform.position.x, transform.position.y, transform.position.z + debug线位置));
        line1.SetPosition(1, new Vector3(transform.position.x, 目标高度, transform.position.z + debug线位置));

        line2.startWidth = 0.1f;
        line2.endWidth = 0.1f;
        line2.positionCount = 2;
        line2.SetPosition(0, new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z + debug线位置));
        line2.SetPosition(1, new Vector3(transform.position.x + 0.5f, transform.position.y + (float)高度控制器.差值修正输出, transform.position.z + debug线位置));

        line3.startWidth = 0.1f;
        line3.endWidth = 0.1f;
        line3.positionCount = 2;
        line3.SetPosition(0, new Vector3(transform.position.x + 1f, transform.position.y, transform.position.z + debug线位置));
        line3.SetPosition(1, new Vector3(transform.position.x + 1f, transform.position.y + (float)高度控制器.偏移修正输出, transform.position.z + debug线位置));

        line4.startWidth = 0.1f;
        line4.endWidth = 0.1f;
        line4.positionCount = 2;
        line4.SetPosition(0, new Vector3(transform.position.x + 1.5f, transform.position.y, transform.position.z + debug线位置));
        line4.SetPosition(1, new Vector3(transform.position.x + 1.5f, transform.position.y + (float)高度控制器.抑制和回正输出, transform.position.z + debug线位置));
    }
    void FixedUpdate() {
        高度控制器.参数.差值修正系数 = 差值修正系数;
        高度控制器.参数.偏移修正速率 = 偏移修正速率;
        高度控制器.参数.自适应远离中心抑制步进帧数间隔 = 自适应远离中心抑制步进帧数间隔;
        高度控制器.参数.抑制步进步进 = 抑制步进步进;
        高度控制器.参数.回正步进步进 = 回正步进步进;
        高度控制器.参数.最大抑制修正步进 = 最大抑制修正步进;
        高度控制器.参数.最大抖动修正量 = 最大抖动修正量;

        高度控制器.使用偏移修正 = 使用偏移修正;
        高度控制器.使用差值修正 = 使用差值修正;
        高度控制器.使用抑制和回正修正 = 使用自适应抖动修正;

        高度控制器.Tick(transform.position.y - 目标高度, Time.deltaTime);

        差值修正输出 = (float)高度控制器.差值修正输出;
        偏移修正输出 = (float)高度控制器.偏移修正输出;
        抑制步进 = (float)高度控制器.抑制步进;
        自适应抖动修正输出 = (float)高度控制器.抑制和回正输出;
        输出 = (float)高度控制器.输出;
    }
}
