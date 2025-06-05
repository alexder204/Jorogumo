using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform player;

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        transform.position = new Vector3(player.position.x, transform.position.y, transform.position.z);
    }
}
