using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace PlayerDialogue
{
    public class MoveScenes2D : MonoBehaviour
    {
        [SerializeField] private string newLevel;
        [SerializeField] private string spawnPointID;

        private bool isTransitioning = false;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (isTransitioning) return;

            if (other.CompareTag("Player"))
            {
                isTransitioning = true;
                SceneTransitionData.spawnPointID = spawnPointID;

                if (SceneFader.instance != null)
                    StartCoroutine(FadeAndLoad());
                else
                    SceneManager.LoadScene(newLevel);
            }
        }

        private IEnumerator FadeAndLoad()
        {
            yield return SceneFader.instance.FadeOutRoutine(); // fade out
            SceneManager.LoadScene(newLevel);                 // load new scene
            // Do not call FadeIn here — SceneFader will handle it automatically on scene load
        }
    }
}
