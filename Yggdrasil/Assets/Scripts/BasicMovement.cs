using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BasicMovement : MonoBehaviour
{
    [Tooltip("Movement")]
    [SerializeField]
    private float moveSpeed = 1f;
    [SerializeField]
    [Range(0f, 1f)]
    private float accelerationMultiplier = 0.1f;
    private Vector3 currentVelocity;
    private Vector3 targetVelocity;
    Transform cam;

    private Rigidbody rb;
    private InputCapture input;
    private int platformLayer;
    private int stairLayer;
    Vector3 startPos;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputCapture>();

        platformLayer = LayerMask.GetMask("Platform");
        stairLayer = LayerMask.GetMask("Stair");
        cam = Camera.main.transform;
        startPos = transform.position;
    }

    private void FixedUpdate()
    {
        //Movement
        float right = input.GetAxis("Player Horizontal");
        float forward = input.GetAxis("Player Forward");
        float vertical = input.GetAxis("Player Vertical");
        Vector3 rightDirection = Vector3.ProjectOnPlane(cam.right, transform.up).normalized;
        Vector3 forwardDirection = Vector3.ProjectOnPlane(cam.forward, transform.up).normalized;

        Vector3 direction = rightDirection * right + forwardDirection * forward + transform.up * vertical;
        if (direction.sqrMagnitude > 1f)
        {
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
    private void Update()
    {
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
        SetColor(Color.white);
        if (input.playing)
        {
            SetColor(Color.green);
        }
        if (input.recording)
        {
            SetColor(Color.red);
        }
    }
    void SetColor(Color color)
    {
        GetComponent<MeshRenderer>().material.color = color;
    }
    void Restart()
    {
        transform.position = startPos;
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        currentVelocity = Vector3.zero;
    }
    public void Move(Vector3 dir)
    {
        targetVelocity = dir * moveSpeed * Time.fixedDeltaTime;
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
        // currentVelocity += Physics.gravity.y * Time.fixedDeltaTime * transform.up.normalized;

        rb.velocity = currentVelocity;
    }

}
