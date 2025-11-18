using UnityEngine;

public class FanRotation : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 0, 500);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}