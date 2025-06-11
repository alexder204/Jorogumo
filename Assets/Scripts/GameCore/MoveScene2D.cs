using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayerDialogue
{
    public class MoveScenes2D : MonoBehaviour
    {
        [SerializeField] private string newLevel;
        [SerializeField] private string spawnPointID; // Set this in inspector for each entrance

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                SceneTransitionData.spawnPointID = spawnPointID;
                SceneManager.LoadScene(newLevel);
            }
        }
    }
}
