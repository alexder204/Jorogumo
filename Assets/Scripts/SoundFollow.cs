using UnityEngine;

public class SoundFollow : MonoBehaviour
{
    private Transform playerTransform;  // Reference to the player
    public AudioSource oceanAudioSource;  // Reference to the ocean sound AudioSource
    public float maxVolumeDistanceX = 10f; // Distance where the sound is loudest on X axis
    public float minVolumeDistanceX = 20f; // Distance where the sound fades out on X axis
    public float maxVolumeDistanceY = 5f;  // Distance where the sound is loudest on Y axis
    public float minVolumeDistanceY = 10f; // Distance where the sound fades out on Y axis

    private void Start()
    {
        // Find player by tag and assign its transform to playerTransform
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        oceanAudioSource = GetComponent<AudioSource>(); // Make sure the AudioSource is attached to the same object as this script
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            // Calculate distances in X and Y direction
            float distanceToPlayerX = Mathf.Abs(transform.position.x - playerTransform.position.x);
            float distanceToPlayerY = Mathf.Abs(transform.position.y - playerTransform.position.y);

            // Adjust the volume based on X-axis distance
            if (distanceToPlayerX < maxVolumeDistanceX)
            {
                oceanAudioSource.volume = 1f; // Max volume when close on X-axis
            }
            else if (distanceToPlayerX > minVolumeDistanceX)
            {
                oceanAudioSource.volume = 0f; // Fade out when too far on X-axis
            }
            else
            {
                oceanAudioSource.volume = 1 - ((distanceToPlayerX - maxVolumeDistanceX) / (minVolumeDistanceX - maxVolumeDistanceX));
            }

            // Adjust the volume based on Y-axis distance
            if (distanceToPlayerY < maxVolumeDistanceY)
            {
                oceanAudioSource.volume = 1f; // Max volume when close on Y-axis
            }
            else if (distanceToPlayerY > minVolumeDistanceY)
            {
                oceanAudioSource.volume = 0f; // Fade out when too far on Y-axis
            }
            else
            {
                oceanAudioSource.volume = Mathf.Max(oceanAudioSource.volume, 1 - ((distanceToPlayerY - maxVolumeDistanceY) / (minVolumeDistanceY - maxVolumeDistanceY)));
            }
        }
    }
}
