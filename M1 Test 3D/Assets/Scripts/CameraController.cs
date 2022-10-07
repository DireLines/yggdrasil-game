using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    //Rotation
    [SerializeField]
    float turnSpeed, vertSpeed, offsetDistance;
    [SerializeField]
    bool turnInverted, vertInverted;
    float turnDirection, vertDirection;
    float camTurn;
    float targetYaw;
    float yawRefVel;
    Vector3 currentUpAxis;

    //Zoom 
    [SerializeField]
    float zoomMax, zoomMin, zoomSensitivity;
    float zoom, zoomSpeed;

    //Positioning
    [SerializeField]
    Transform target;
    public Transform Target { get => target; set => target = value; }

    [SerializeField]
    float lerpPercentage = 0.01f;

    private Vector3 targetPos;
    Vector3 velocityRef;
    private float yaw, pitch;
    Quaternion targetRotation;


    void Start() {
        turnDirection = turnInverted ? 1 : -1;
        vertDirection = vertInverted ? -1 : 1;
        targetPos = target.position;

        yaw = 0f;
        targetYaw = 0f;

        pitch = 0f;

        camTurn = 0f;

        currentUpAxis = Target.up.normalized;
    }

    void FixedUpdate() {
        //Rotation
        camTurn = Input.GetAxisRaw("Camera Horizontal");
        targetYaw += turnDirection * 2f * camTurn;

        yaw = Mathf.SmoothDamp(yaw, targetYaw, ref yawRefVel, 1 / turnSpeed);
        pitch += vertSpeed * 45f * vertDirection * Input.GetAxisRaw("Camera Vertical") * Time.fixedDeltaTime;
        pitch = Mathf.Clamp(pitch, 5, 70);
        Quaternion pitchyaw = Quaternion.Euler(new Vector3(pitch, yaw));
        currentUpAxis = Vector3.Slerp(currentUpAxis, Target.up.normalized, lerpPercentage);
        transform.rotation = Quaternion.FromToRotation(Vector3.up, currentUpAxis) * pitchyaw;

        //Zoom
        zoom *= 1f - Input.GetAxisRaw("Zoom") * zoomSensitivity * Time.fixedDeltaTime;
        zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);

        //Positioning
        targetPos = Vector3.SmoothDamp(targetPos, Target.position, ref velocityRef, lerpPercentage);
        transform.position = targetPos - transform.forward * zoom;
    }
}
