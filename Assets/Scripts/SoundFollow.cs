using UnityEngine;

public class SoundFollow : MonoBehaviour
{
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            // Keep current Y and Z, but match player's X position
            transform.position = new Vector3(
                playerTransform.position.x,
                transform.position.y,
                transform.position.z
            );
        }
    }
}