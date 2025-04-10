using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; 
    public Vector3 offset;   
    public float smoothSpeed = 0.125f; 

    public float minX, maxX, minY, maxY, minZ, maxZ;

    private void FixedUpdate()
    {
        Vector3 desiredPosition = player.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        float clampedX = Mathf.Clamp(smoothedPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(smoothedPosition.y, minY, maxY);
        float clampedZ = Mathf.Clamp(smoothedPosition.z, minZ, maxZ);

        transform.position = new Vector3(clampedX, clampedY, clampedZ);
    }
}
