using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerSpawnManager : MonoBehaviour
{
    public Transform playerTransform;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(MovePlayerToSpawnPoint());
    }

    private IEnumerator MovePlayerToSpawnPoint()
    {
        TopDownMovement.isSceneLoading = true;

        yield return null;  // wait one frame for the scene to initialize

        if (playerTransform == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogWarning("[PlayerSpawnManager] Player tag not found.");
                yield break;
            }
            playerTransform = playerObj.transform;
        }

        // hide player to avoid any flash at default position
        playerTransform.gameObject.SetActive(false);

        // position player
        if (!string.IsNullOrEmpty(SceneTransitionData.spawnPointID))
        {
            var spawns = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            foreach (var sp in spawns)
            {
                if (sp.spawnPointID == SceneTransitionData.spawnPointID)
                {
                    playerTransform.position = sp.transform.position;
                    break;
                }
            }
            SceneTransitionData.spawnPointID = null;
        }

        // show player
        playerTransform.gameObject.SetActive(true);

        // ensure one more frame before fade-in (optional but stabilizes)
        yield return null;

        // fade in
        if (SceneFader.instance != null)
            yield return SceneFader.instance.FadeIn();

        TopDownMovement.isSceneLoading = false;
    }
}
