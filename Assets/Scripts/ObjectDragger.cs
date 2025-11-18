using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectDragger : MonoBehaviour
{
    private Color originalColor;
    private Renderer rend;
    private Rigidbody rb;
    private Vector3 offset;
    private float mouseZCoord;
    private float originalDrag;
    private bool isDragging;

    private const int Samples = 6;
    private Vector3[] worldVelBuf = new Vector3[Samples];
    private int velIdx = 0;
    private Vector3 lastWorldUnderMouse;
    private Vector2 lastMouse;
    private Vector2[] screenVelBuf = new Vector2[Samples];

    [Header("Drag Settings")]
    public Color dragColor = Color.cyan;
    public float followSpeed = 15f;
    public float dragOnDrag = 2f;

    [Header("Depth Control")]
    public float depthScrollSpeed = 2.5f;
    public KeyCode depthDragKey = KeyCode.LeftShift;
    public float depthDragSensitivity = 0.08f;

    [Header("Gravity Feel")]
    public float gravityScale = 2.0f;
    public float releaseDownwardBoost = 0f;

    [Header("Throw")]
    public float throwMultiplier = 1.2f;
    public float depthCompensation = 1.5f;
    public float forwardThrowGain = 0.012f;
    public float minDepth = 0.3f;
    public float maxDepth = 200f;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        if (rend != null) originalColor = rend.material.color;
        rb.useGravity = true;
        rb.linearDamping = 0f;
        originalDrag = rb.linearDamping;
    }

    void FixedUpdate()
    {
        if (!isDragging && gravityScale != 1f)
        {
            Vector3 extra = (gravityScale - 1f) * Physics.gravity;
            rb.AddForce(extra, ForceMode.Acceleration);
        }
    }

    void OnMouseDown()
    {
        if (rend != null) rend.material.color = dragColor;

        var cam = Camera.main;
        mouseZCoord = cam.WorldToScreenPoint(transform.position).z;
        offset = transform.position - GetMouseWorldPos();

        isDragging = true;
        rb.useGravity = false;
        rb.linearDamping = dragOnDrag;

        lastWorldUnderMouse = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mouseZCoord));
        lastMouse = Input.mousePosition;
        velIdx = 0;
        for (int i = 0; i < Samples; i++) { worldVelBuf[i] = Vector3.zero; screenVelBuf[i] = Vector2.zero; }
    }

    void OnMouseDrag()
    {
        var cam = Camera.main;
        float nearZ = cam.nearClipPlane + 0.05f;
        float farZ = cam.farClipPlane - 0.05f;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0f)
            mouseZCoord += scroll * depthScrollSpeed;

        if (Input.GetKey(depthDragKey))
            mouseZCoord -= Input.GetAxis("Mouse Y") * depthDragSensitivity;

        mouseZCoord = Mathf.Clamp(mouseZCoord, nearZ, farZ);

        Vector3 worldUnderMouse = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mouseZCoord));
        Vector3 target = worldUnderMouse + offset;

        Vector3 v = (target - rb.position) * followSpeed;
        rb.linearVelocity = v;

        float dt = Mathf.Max(Time.deltaTime, 1e-4f);
        Vector3 wv = (worldUnderMouse - lastWorldUnderMouse) / dt;
        Vector2 sv = ((Vector2)Input.mousePosition - lastMouse) / dt;

        worldVelBuf[velIdx] = wv;
        screenVelBuf[velIdx] = sv;
        velIdx = (velIdx + 1) % Samples;

        lastWorldUnderMouse = worldUnderMouse;
        lastMouse = Input.mousePosition;
    }

    void OnMouseUp()
    {
        if (rend != null) rend.material.color = originalColor;

        var cam = Camera.main;
        float dist = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward);
        float depthFactor = depthCompensation / Mathf.Clamp(dist, minDepth, maxDepth);

        Vector3 avgWV = Vector3.zero;
        Vector2 avgSV = Vector2.zero;
        for (int i = 0; i < Samples; i++)
        {
            avgWV += worldVelBuf[i];
            avgSV += screenVelBuf[i];
        }
        avgWV /= Samples;
        avgSV /= Samples;

        Vector3 forwardAdd = cam.transform.forward * Mathf.Max(0f, avgSV.y) * forwardThrowGain;

        rb.linearVelocity = avgWV * throwMultiplier * depthFactor + forwardAdd;

        rb.useGravity = true;
        rb.linearDamping = originalDrag;

        if (releaseDownwardBoost > 0f)
            rb.linearVelocity += Vector3.down * releaseDownwardBoost;

        isDragging = false;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mp = Input.mousePosition;
        mp.z = mouseZCoord;
        return Camera.main.ScreenToWorldPoint(mp);
    }
}
