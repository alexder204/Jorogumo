using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayerDialogue
{
    public class MoveScenes2D : MonoBehaviour
    {
        [SerializeField] private string newLevel;
        [SerializeField] private string spawnPointID;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                SceneTransitionData.spawnPointID = spawnPointID;

                if (SceneFader.instance != null)
                {
                    SceneFader.instance.FadeOutAndLoad(newLevel);
                }
                else
                {
                    // fallback if no fader found
                    SceneManager.LoadScene(newLevel);
                }
            }
        }
    }
}
