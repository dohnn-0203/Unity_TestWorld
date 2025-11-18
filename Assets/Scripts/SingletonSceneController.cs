using UnityEngine;

public class SingletonSceneController : MonoBehaviour
{
    public static SingletonSceneController instance;
    public Camera sceneCamera;
    public float dragLerp = 20f;
    public LayerMask raycastMask = ~0;

    private GameObject selectedObject;
    private float objectCameraDistance;
    private Vector3 offset;
    private Rigidbody selectedRb;
    private bool prevUseGravity;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void HandlePointerDown(Vector2 viewportPos)
    {
        if (sceneCamera == null) return;
        Ray ray = sceneCamera.ViewportPointToRay(viewportPos);
        if (!Physics.Raycast(ray, out var hit, 1000f, raycastMask, QueryTriggerInteraction.Ignore)) return;

        selectedObject = hit.transform.gameObject;
        selectedRb = selectedObject.GetComponent<Rigidbody>();
        if (selectedRb != null)
        {
            prevUseGravity = selectedRb.useGravity;
            selectedRb.useGravity = false;
        }

        selectedObject.GetComponent<TouchableObject>()?.OnInteract();

        objectCameraDistance = Vector3.Distance(sceneCamera.transform.position, hit.point);
        var initialWorld = sceneCamera.ViewportToWorldPoint(new Vector3(viewportPos.x, viewportPos.y, objectCameraDistance));
        offset = selectedObject.transform.position - initialWorld;
    }

    public void HandleDrag(Vector2 viewportPos)
    {
        if (sceneCamera == null || selectedObject == null) return;
        var targetWorld = sceneCamera.ViewportToWorldPoint(new Vector3(viewportPos.x, viewportPos.y, objectCameraDistance)) + offset;

        if (selectedRb != null && selectedRb.isKinematic == false)
        {
            var next = Vector3.Lerp(selectedRb.position, targetWorld, Time.deltaTime * dragLerp);
            selectedRb.MovePosition(next);
            selectedRb.linearVelocity = Vector3.zero;
            selectedRb.angularVelocity = Vector3.zero;
        }
        else
        {
            selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, targetWorld, Time.deltaTime * dragLerp);
        }
    }

    public void HandlePointerUp()
    {
        if (selectedRb != null) selectedRb.useGravity = prevUseGravity;
        selectedObject = null;
        selectedRb = null;
    }
}
