using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AircraftPhysics : MonoBehaviour {
    const float PREDICTION_TIMESTEP_FRACTION = 0.5f;

    [SerializeField]
    float thrust = 0;
    [SerializeField]
    public List<AeroSurface> aerodynamicSurfaces = null;

    Rigidbody rb;
    float thrustPercent;
    BiVector3 currentForceAndTorque;

    public void SetThrustPercent(float percent) {
        thrustPercent = percent;
    }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        BiVector3 forceAndTorqueThisFrame =
            CalculateAerodynamicForces(rb.velocity, rb.angularVelocity, Vector3.zero, 1.2f, rb.worldCenterOfMass);

        Vector3 velocityPrediction = PredictVelocity(forceAndTorqueThisFrame.p
            + transform.forward * thrust * thrustPercent + Physics.gravity * rb.mass);

        Vector3 angularVelocityPrediction = PredictAngularVelocity(forceAndTorqueThisFrame.q);


        BiVector3 forceAndTorquePrediction =
            CalculateAerodynamicForces(velocityPrediction, angularVelocityPrediction, Vector3.zero, 1.2f, rb.worldCenterOfMass);

        currentForceAndTorque = (forceAndTorqueThisFrame + forceAndTorquePrediction) * 0.5f;

        rb.AddForce(currentForceAndTorque.p);
        rb.AddTorque(currentForceAndTorque.q);

        rb.AddForce(transform.forward * thrust * thrustPercent);
    }

    private BiVector3 CalculateAerodynamicForces(Vector3 velocity, Vector3 angularVelocity, Vector3 wind, float airDensity, Vector3 centerOfMass) {
        BiVector3 forceAndTorque = new BiVector3();
        foreach (var surface in aerodynamicSurfaces) {
            if (surface == null) continue;
            Vector3 relativePosition = surface.transform.position - centerOfMass;
            var add = surface.CalculateForces(
                -velocity + wind
                - Vector3.Cross(angularVelocity,
                relativePosition),
                airDensity, relativePosition);
            //对于add里的所有数据，如果是无限或者nan，那么就换为0，然后再进行累加
            if (float.IsInfinity(add.p.x) || float.IsInfinity(add.p.y) || float.IsInfinity(add.p.z) ||
                float.IsNaN(add.p.x) || float.IsNaN(add.p.y) || float.IsNaN(add.p.z)) {
                add.p = Vector3.zero;
            }
            if (float.IsInfinity(add.q.x) || float.IsInfinity(add.q.y) || float.IsInfinity(add.q.z) ||
                float.IsNaN(add.q.x) || float.IsNaN(add.q.y) || float.IsNaN(add.q.z)) {
                add.q = Vector3.zero;
            }
            forceAndTorque += add;
        }
        return forceAndTorque;
    }

    private Vector3 PredictVelocity(Vector3 force) {
        return rb.velocity + Time.fixedDeltaTime * PREDICTION_TIMESTEP_FRACTION * force / rb.mass;
    }

    private Vector3 PredictAngularVelocity(Vector3 torque) {
        Quaternion inertiaTensorWorldRotation = rb.rotation * rb.inertiaTensorRotation;
        Vector3 torqueInDiagonalSpace = Quaternion.Inverse(inertiaTensorWorldRotation) * torque;
        Vector3 angularVelocityChangeInDiagonalSpace;
        angularVelocityChangeInDiagonalSpace.x = torqueInDiagonalSpace.x / rb.inertiaTensor.x;
        angularVelocityChangeInDiagonalSpace.y = torqueInDiagonalSpace.y / rb.inertiaTensor.y;
        angularVelocityChangeInDiagonalSpace.z = torqueInDiagonalSpace.z / rb.inertiaTensor.z;

        return rb.angularVelocity + Time.fixedDeltaTime * PREDICTION_TIMESTEP_FRACTION
            * (inertiaTensorWorldRotation * angularVelocityChangeInDiagonalSpace);
    }

#if UNITY_EDITOR
    // For gizmos drawing.
    public void CalculateCenterOfLift(out Vector3 center, out Vector3 force, Vector3 displayAirVelocity, float displayAirDensity) {
        Vector3 com;
        BiVector3 forceAndTorque;
        if (aerodynamicSurfaces == null) {
            center = Vector3.zero;
            force = Vector3.zero;
            return;
        }

        if (rb == null) {
            com = GetComponent<Rigidbody>().worldCenterOfMass;
            forceAndTorque = CalculateAerodynamicForces(-displayAirVelocity, Vector3.zero, Vector3.zero, displayAirDensity, com);
        } else {
            com = rb.worldCenterOfMass;
            forceAndTorque = currentForceAndTorque;
        }

        force = forceAndTorque.p;
        center = com + Vector3.Cross(forceAndTorque.p, forceAndTorque.q) / forceAndTorque.p.sqrMagnitude;
    }
#endif
}


