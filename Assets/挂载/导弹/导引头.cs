using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using static TGZG.公共空间;
using static 战雷革命.公共空间;
using 战雷革命;

public class 导引头 : MonoBehaviour {
    Rigidbody rb;

    [HideInInspector]
    public 近炸控制器 近炸控制器;
    [HideInInspector]
    public 爆炸控制器 爆炸控制器;
    [HideInInspector]
    public 导弹翼面控制 翼面控制器;
    public float 预热时间;
    [Header("角度")]
    public float 离轴角度;
    public float 视场;
    [Header("追踪")]
    public float 太阳回避角;
    public float 追踪率;
    public float 僵直时间;
    [Header("锁定")]
    public float 锁定敏感度;
    public bool 锁定后缩视场 = false;
    public float 锁定后视场;
    public bool 干扰物闭眼 = false;
    [Header("战斗部")]
    public float 自爆时间;
    public float 脱锁后自爆时间;
    public float 近炸延迟时间;
    [HideInInspector]
    public Vector2 导引头转动角度 = Vector2.zero;
    [HideInInspector]
    public bool 导引头锁定 = false;
    [HideInInspector]
    public bool 闭眼 = false;


    public bool 已启动 = false;
    Stopwatch 预热计时器 = new Stopwatch();
    Stopwatch 发射计时器 = new Stopwatch();


    void Start() {
        rb = GetComponent<Rigidbody>();
        近炸控制器 = GetComponent<近炸控制器>();
        爆炸控制器 = GetComponent<爆炸控制器>();
        翼面控制器 = GetComponent<导弹翼面控制>();
    }

    void FixedUpdate() {
        if (!已启动) return;
        if (预热计时器.Elapsed.TotalSeconds < 预热时间) return;
        var 红外信号 = 导引头扫描();
        //获取视场内的红外信号
        var 信号数据 = 红外信号.Where(t => {
            var 导引头方向 = 导引头转动角度;
            var 目标方向 = t.角度;
            var 角度 = Mathf.Max(Mathf.Abs(导引头方向.x - 目标方向.x), Mathf.Abs(导引头方向.y - 目标方向.y));
            if (锁定后缩视场 && 导引头锁定) {
                return 角度 < 锁定后视场;
            }
            return 角度 < 视场;
        });
        //视场转动
        var 最大转动速率 = 追踪率 / Time.fixedDeltaTime;
        if (信号数据.Count() == 0) {
            导引头锁定 = false;
            导引头转动角度 = Vector2.MoveTowards(导引头转动角度, Vector2.zero, 最大转动速率);
        } else {
            导引头锁定 = true;
            //获得视场内红外信号的平均角度
            var x平均角度 = 信号数据.Average(t => t.角度.x);
            var y平均角度 = 信号数据.Average(t => t.角度.y);
            var 导引头目标角度 = new Vector2(x平均角度, y平均角度);
            //计算导引头转动角度
            导引头转动角度 = Vector2.MoveTowards(导引头转动角度, 导引头目标角度, 最大转动速率);
        }
        //自爆计时
        if (发射计时器.Elapsed.TotalSeconds > 自爆时间) {
            爆炸控制器.爆炸();
        }
        //近炸计时
        if (发射计时器.Elapsed.TotalSeconds > 近炸延迟时间) {
            近炸控制器.启动();
        }
        //控制方向
        偏转(导引头转动角度);
    }
    /// <summary>
    /// 返回扫描到的红外信号，按强度排序
    /// </summary>
    List<红外信号数据> 导引头扫描() {
        List<红外信号数据> 信号数据 = new List<红外信号数据>();
        var 所有对象 = GameObject.FindObjectsOfType<红外信号源>();
        foreach (var i in 所有对象) {
            var 对象 = i.gameObject;
            //排除自己
            if (对象 == gameObject) continue;
            //只获取红外源
            //var 红外源 = 对象.GetComponent<红外信号源>();
            //if (红外源 == null) continue;
            //判断是否在视野内
            var 目标坐标本地 = transform.InverseTransformPoint(i.transform.position);
            var 角度 = Vector3.Angle(目标坐标本地, Vector3.forward);
            if (角度 > 离轴角度) continue;
            //障碍物看不见
            RaycastHit hit;
            if (Physics.Raycast(transform.position, (i.transform.position - transform.position).normalized, out hit)) {
                //渲染射线
                UnityEngine.Debug.DrawLine(transform.position, i.transform.position, Color.red);
                //射线检测到非红外源且非自己，视为障碍物
                if (hit.transform.gameObject != i.gameObject && hit.transform.gameObject != gameObject) {
                    //输出名称
                    $"红外源：{i.gameObject.name} 障碍物：{hit.transform.gameObject.name}".log();
                    continue;
                }
            }
            //筛掉低强度信号
            var 距离 = 目标坐标本地.magnitude;
            var 信号强度 = i.初始强度(transform.position) / 距离;
            if (信号强度 < 锁定敏感度) continue;
            //返回信号数据
            var Y轴角度 = Vector3.Angle(new Vector3(0, 目标坐标本地.y, 目标坐标本地.z), Vector3.forward) * Mathf.Sign(目标坐标本地.y);
            var X轴角度 = Vector3.Angle(new Vector3(目标坐标本地.x, 0, 目标坐标本地.z), Vector3.forward) * Mathf.Sign(目标坐标本地.x);
            var 信号 = new 红外信号数据() {
                角度 = new Vector2(X轴角度, Y轴角度),
                强度 = 信号强度,
                类型 = i.类型,
            };
            信号数据.Add(信号);
        }
        return 信号数据.OrderByDescending(t => t.强度).ToList();
    }
    void 偏转(Vector2 角度) {
        if (发射计时器.Elapsed.TotalSeconds < 僵直时间) return;
        翼面控制器.控制导弹翼面(角度);
    }

    public void 开始发射计时() {
        启动();
        发射计时器.Start();
    }
    public void 启动() {
        已启动 = true;
        预热计时器.Start();
    }
    public void 停止() {
        已启动 = false;
        导引头转动角度 = Vector2.zero;
        导引头锁定 = false;
        预热计时器.Stop();
        发射计时器.Stop();
        预热计时器.Reset();
        发射计时器.Reset();
    }
}
struct 红外信号数据 {
    public Vector2 角度;
    public float 强度;
    public 信号类型 类型;
}
