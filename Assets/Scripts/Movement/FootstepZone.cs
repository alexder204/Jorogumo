using UnityEngine;

public class FootstepZone : MonoBehaviour
{
    [System.Serializable]
    public class SurfaceAudio
    {
        public string surfaceName;
        public AudioClip[] footstepClips;
    }

    public SurfaceAudio[] surfaces;

    [Tooltip("Must match a surface name in the list above.")]
    public string surfaceType;

    [Tooltip("Higher priority wins when zones overlap.")]
    public int priority;

    public AudioClip GetRandomClip(string surfaceName)
    {
        if (surfaces == null)
            return null;

        foreach (SurfaceAudio surface in surfaces)
        {
            if (surface == null || surface.surfaceName != surfaceName || surface.footstepClips == null || surface.footstepClips.Length == 0)
                continue;

            return surface.footstepClips[Random.Range(0, surface.footstepClips.Length)];
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        foreach (Collider2D area in GetComponentsInChildren<Collider2D>())
        {
            // Only draw colliders belonging to this zone.
            if (area.GetComponentInParent<FootstepZone>() != this)
                continue;

            Gizmos.DrawWireCube(area.bounds.center, area.bounds.size);
        }
    }
}