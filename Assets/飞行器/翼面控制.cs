using System.Collections.Generic;
using TGZG;
using UnityEngine;
using UnityEngine.UI;

public class 翼面控制 : MonoBehaviour {
    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
    [SerializeField]
    List<WheelCollider> wheels = null;
    [SerializeField]
    float rollControlSensitivity = 0.2f;
    [SerializeField]
    float pitchControlSensitivity = 0.2f;
    [SerializeField]
    float yawControlSensitivity = 0.2f;

    [Range(-1, 1)]
    public float 俯仰;
    [Range(-1, 1)]
    public float 偏航;
    [Range(-1, 1)]
    public float 滚转;
    [Range(0, 1)]
    public float 襟翼;

    public float 速度;

    float 俯仰_飞控输入;
    float 偏航_飞控输入;
    float 滚转_飞控输入;
    float _襟翼;

    public void 控制襟翼(bool 开) {
        if (开) {
            _襟翼 = 襟翼开启值;
        } else {
            _襟翼 = 0;
        }
    }
    public void 飞控接口((float 俯仰, float 偏航, float 滚转) 控制数据) {
        俯仰_飞控输入 = 控制数据.俯仰;
        滚转_飞控输入 = 控制数据.滚转;
        偏航_飞控输入 = 控制数据.偏航;
    }
    public float 襟翼开启值 = 0.3f;

    public float 节流阀 = 0;
    public float 刹车 = 0;

    AircraftPhysics aircraftPhysics;
    Rigidbody rb;

    private void Start() {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        //玩家键盘操作为最高优先级，其次飞控。
        if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Yaw") != 0) {
            俯仰 = Input.GetAxis("Vertical");
            偏航 = Input.GetAxis("Yaw");
        } else {
            俯仰 = 俯仰_飞控输入;
            偏航 = 偏航_飞控输入;
        }
        滚转 = Input.GetAxis("Horizontal") == 0 ? 滚转_飞控输入 : Input.GetAxis("Horizontal");
        襟翼 = _襟翼;
        if (Input.GetKey(KeyCode.LeftShift) && 节流阀 < 1.0f) {
            节流阀 += 0.01f;
        }
        if (Input.GetKey(KeyCode.LeftControl) && 节流阀 > 0.0f) {
            节流阀 -= 0.01f;
        }
        //滚轮节流阀
        节流阀 += Input.mouseScrollDelta.y / 50;
        节流阀 = Mathf.Clamp(节流阀, 0, 1);
        //F襟翼
        if (Input.GetKeyDown(KeyCode.F)) {
            控制襟翼(襟翼 == 0);
        }
        //B刹车
        if (Input.GetKeyDown(KeyCode.B)) {
            刹车 = 刹车 > 0 ? 0 : 100f;
        }
    }

    private void FixedUpdate() {
        SetControlSurfecesAngles(俯仰, 滚转, 偏航, 襟翼);
        aircraftPhysics.SetThrustPercent(节流阀);
        foreach (var wheel in wheels) {
            wheel.brakeTorque = 刹车;
            // small torque to wake up wheel collider
            wheel.motorTorque = 0.01f;
        }
        速度 = rb.velocity.magnitude;
    }

    public void SetControlSurfecesAngles(float pitch, float roll, float yaw, float flap) {
        foreach (var surface in controlSurfaces) {
            if (surface == null || !surface.IsControlSurface) continue;
            switch (surface.InputType) {
                case ControlInputType.Pitch:
                surface.SetFlapAngle(pitch * pitchControlSensitivity * surface.InputMultiplyer);
                break;
                case ControlInputType.Roll:
                surface.SetFlapAngle(roll * rollControlSensitivity * surface.InputMultiplyer);
                break;
                case ControlInputType.Yaw:
                surface.SetFlapAngle(yaw * yawControlSensitivity * surface.InputMultiplyer);
                break;
                case ControlInputType.Flap:
                surface.SetFlapAngle(襟翼 * surface.InputMultiplyer);
                break;
            }
        }
    }

    private void OnDrawGizmos() {
        if (!Application.isPlaying)
            SetControlSurfecesAngles(俯仰, 滚转, 偏航, 襟翼);
    }
}
