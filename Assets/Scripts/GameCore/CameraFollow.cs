using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 0, -10);

    public float minX, maxX, minY, maxY;

    public float deadzoneWidth = 2f;
    public float deadzoneHeight = 2f;

    private void LateUpdate()
    {
        Vector3 cameraPos = transform.position;
        Vector3 targetPos = player.position + offset;

        // Calculate deadzone bounds
        float leftBound = cameraPos.x - deadzoneWidth / 2;
        float rightBound = cameraPos.x + deadzoneWidth / 2;
        float bottomBound = cameraPos.y - deadzoneHeight / 2;
        float topBound = cameraPos.y + deadzoneHeight / 2;

        Vector3 newCameraPos = cameraPos;

        // Only move the camera if the player leaves the deadzone
        if (targetPos.x < leftBound)
            newCameraPos.x = targetPos.x + deadzoneWidth / 2;
        else if (targetPos.x > rightBound)
            newCameraPos.x = targetPos.x - deadzoneWidth / 2;

        if (targetPos.y < bottomBound)
            newCameraPos.y = targetPos.y + deadzoneHeight / 2;
        else if (targetPos.y > topBound)
            newCameraPos.y = targetPos.y - deadzoneHeight / 2;

        // Clamp to map bounds
        newCameraPos.x = Mathf.Clamp(newCameraPos.x, minX, maxX);
        newCameraPos.y = Mathf.Clamp(newCameraPos.y, minY, maxY);
        newCameraPos.z = offset.z; // Ensure proper depth

        transform.position = newCameraPos;
    }
}
