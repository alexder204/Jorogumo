using UnityEngine;

public class CameraAutoScale : MonoBehaviour
{
    public float defaultOrthographicSize = 5f; // Default for 1080p
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    private void Start()
    {
        float screenRatio = (float)Screen.height / referenceResolution.y;
        Camera.main.orthographicSize = defaultOrthographicSize / screenRatio;
    }
}
