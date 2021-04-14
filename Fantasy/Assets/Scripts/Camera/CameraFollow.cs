using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Player player;
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    public float cameraHeight = 8f;
    public float cameraZoffset = 2f;

    private void FixedUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

}
