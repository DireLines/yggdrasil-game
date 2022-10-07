using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    [Tooltip("Movement")]
    [SerializeField]
    private float moveSpeed = 1f;
    [SerializeField]
    [Range(0f, 1f)]
    private float accelerationMultiplier = 0.1f;
    private Vector3 currentVelocity;
    private Vector3 targetVelocity;
    Transform cam;

    const float ceilingHeight = 9000;
    const float reallySmall = 0.001f;

    private Rigidbody rb;
    private InputCapture input;
    private int platformLayer;
    private int stairLayer;
    Vector3 startPos;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputCapture>();

        platformLayer = LayerMask.GetMask("Platform");
        stairLayer = LayerMask.GetMask("Stair");
        cam = Camera.main.transform;
        startPos = transform.position;
    }

    private void FixedUpdate() {
        //Movement
        float right = input.GetAxis("Player Horizontal");
        float forward = input.GetAxis("Player Forward");
        Vector3 rightDirection = Vector3.ProjectOnPlane(cam.right, transform.up).normalized;
        Vector3 forwardDirection = Vector3.ProjectOnPlane(cam.forward, transform.up).normalized;

        Vector3 direction = rightDirection * right + forwardDirection * forward;
        if (direction.sqrMagnitude > 1f) {
            direction.Normalize();
        }
        Move(direction);
        //Inventory
        // if (Input.mouseScrollDelta.y > 0f) {
        //     inventory.NextItem();
        // } else if (Input.mouseScrollDelta.y < 0f) {
        //     inventory.PreviousItem();
        // }
    }
    private void Update() {
        // if (Input.GetKeyDown(KeyCode.R)) {
        //     if (input.HasSequence("seq1")) {
        //         input.StopRecord();
        //         input.StartPlayback("seq1");
        //     } else {
        //         input.StartRecord("seq1");
        //     }
        // }
        // if (Input.GetKeyDown(KeyCode.G)) {
        //     SwitchGravity();
        // }
        if (Input.GetKeyDown(KeyCode.R)) {
            Restart();
        }
        SetColor(Color.white);
        if (input.playing) {
            SetColor(Color.green);
        }
        if (input.recording) {
            SetColor(Color.red);
        }
    }
    void SetColor(Color color) {
        GetComponent<MeshRenderer>().material.color = color;
    }
    void SwitchGravity(Vector3 newUp) {
        transform.up = newUp;
    }
    void Restart() {
        transform.position = startPos;
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        currentVelocity = Vector3.zero;
    }
    private void OnCollisionEnter(Collision other) {
        SwitchGravity(other.transform.up);
        rb.velocity = Vector3.zero;
        currentVelocity = Vector3.zero;
    }
    private void OnCollisionStay(Collision other) {
        SwitchGravity(other.transform.up);
    }
    public void Move(Vector3 dir) {
        targetVelocity = dir * moveSpeed * Time.fixedDeltaTime + transform.up * Vector3.Dot(transform.up, rb.velocity);
        currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref currentVelocity, accelerationMultiplier / 10f, moveSpeed);

        // if (currentVelocity.sqrMagnitude > reallySmall * reallySmall) {
        //     transform.forward = Vector3.ProjectOnPlane(currentVelocity, transform.up);
        // }

        // RaycastHit hit;
        // bool abovePlatform = Physics.BoxCast(transform.position + transform.localScale.y * transform.up.normalized, transform.localScale,
        //                     -transform.up, out hit, transform.rotation, Mathf.Infinity, platformLayer | stairLayer);
        // if (abovePlatform) {
        //     if (Mathf.Abs(hit.point.y - (transform.position.y - transform.localScale.y / 2f)) < transform.localScale.y) {
        //         rb.position = new Vector3(rb.position.x, hit.point.y + transform.localScale.y / 2f, rb.position.z);
        //         currentVelocity.y = 0f;
        //     } else {
        //         currentVelocity += Physics.gravity.y * Time.fixedDeltaTime * transform.up.normalized;
        //     }
        // } else {
        //     currentVelocity += Physics.gravity.y * Time.fixedDeltaTime * transform.up.normalized;
        // }
        currentVelocity += Physics.gravity.y * Time.fixedDeltaTime * transform.up.normalized;

        rb.velocity = currentVelocity;
    }

}
