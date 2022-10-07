using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceshipController : MonoBehaviour {
    private Vector3 thrusterInput;
    private Quaternion targetRot;
    private Quaternion smoothedRot;
    Transform cam;

    public float thrustStrength = 2;
    public float rotSpeed = 0.4f;
    public float rollSpeed = 30;
    public float rotSmoothSpeed = 10;

    KeyCode downKey = KeyCode.Q;
    KeyCode upKey = KeyCode.E;
    KeyCode leftKey = KeyCode.A;
    KeyCode rightKey = KeyCode.D;
    KeyCode backwardKey = KeyCode.S;
    KeyCode forwardKey = KeyCode.W;
    KeyCode turnLeftKey = KeyCode.LeftArrow;
    KeyCode turnRightKey = KeyCode.RightArrow;
    KeyCode turnUpKey = KeyCode.UpArrow;
    KeyCode turnDownKey = KeyCode.DownArrow;
    KeyCode rollCounterKey = KeyCode.Z;
    KeyCode rollClockwiseKey = KeyCode.C;

    private Rigidbody rb;
    private InputCapture input;

    int numCollisions = 0;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputCapture>();
        cam = Camera.main.transform;
        thrusterInput = Vector3.zero;
        targetRot = transform.rotation;
        smoothedRot = transform.rotation;
    }
    private void Update() {
        HandleMovement();
    }
    private void OnCollisionEnter(Collision other) {
        numCollisions++;
    }
    private void OnCollisionExit(Collision other) {
        numCollisions--;
    }
    void SetColor(Color color) {
        GetComponent<MeshRenderer>().material.color = color;
    }
    int GetInputAxis(KeyCode negativeAxis, KeyCode positiveAxis) {
        int axis = 0;
        if (input.GetKey(positiveAxis)) {
            axis++;
        }
        if (input.GetKey(negativeAxis)) {
            axis--;
        }
        return axis;
    }
    void HandleMovement() {
        int thrustInputX = GetInputAxis(leftKey, rightKey);
        int thrustInputY = GetInputAxis(downKey, upKey);
        int thrustInputZ = GetInputAxis(backwardKey, forwardKey);
        thrusterInput = new Vector3(thrustInputX, thrustInputY, thrustInputZ);
        float yawInput = GetInputAxis(turnLeftKey, turnRightKey) * rotSpeed;
        float pitchInput = GetInputAxis(turnDownKey, turnUpKey) * rotSpeed;
        float rollInput = GetInputAxis(rollCounterKey, rollClockwiseKey) * rollSpeed * Time.fixedDeltaTime;
        Quaternion yaw = Quaternion.AngleAxis(yawInput, transform.up);
        Quaternion pitch = Quaternion.AngleAxis(-pitchInput, transform.right);
        Quaternion roll = Quaternion.AngleAxis(-rollInput, transform.forward);
        if (numCollisions != 0) {
            targetRot = transform.rotation;
            smoothedRot = transform.rotation;
            return;
        }
        targetRot = yaw * pitch * roll * targetRot;
        smoothedRot = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * rotSmoothSpeed);
    }

    private void FixedUpdate() {
        Vector3 thrustDir = transform.TransformVector(thrusterInput);
        rb.AddForce(thrustDir * thrustStrength, ForceMode.Acceleration);
        if (numCollisions == 0) {
            rb.MoveRotation(smoothedRot.normalized);
        }
    }
}
