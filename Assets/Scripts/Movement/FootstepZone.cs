using UnityEngine;

public class FootstepZone : MonoBehaviour
{
    [System.Serializable]
    public class SurfaceAudio
    {
        public string surfaceName;              // e.g. "Grass", "Pavement"
        public AudioClip[] footstepClips;       // Footstep sounds for this surface
    }

    public SurfaceAudio[] surfaces;

    // This method is called by the player to get footstep sounds
    public AudioClip GetRandomClip(string surfaceName)
    {
        foreach (var surface in surfaces)
        {
            if (surface.surfaceName == surfaceName && surface.footstepClips.Length > 0)
            {
                return surface.footstepClips[Random.Range(0, surface.footstepClips.Length)];
            }
        }

        return null;
    }

    [Tooltip("The type of surface this zone represents (must match one of the SurfaceAudio names above).")]
    public string surfaceType;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size);
    }
}
