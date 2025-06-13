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
        yield return null; // wait a frame

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
            else
            {
                Debug.LogWarning("Player GameObject with tag 'Player' not found.");
                yield break;
            }
        }

        if (string.IsNullOrEmpty(SceneTransitionData.spawnPointID))
        {
            Debug.Log("No spawnPointID set for this scene load. Spawning Default");
            yield break;
        }

        SpawnPoint[] spawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        foreach (var sp in spawnPoints)
        {
            if (sp.spawnPointID == SceneTransitionData.spawnPointID)
            {
                playerTransform.position = sp.transform.position;
                break;
            }
        }

        SceneTransitionData.spawnPointID = null;
    }
}
