using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader instance;

    [Tooltip("Assign the Image on your persistent Canvas (tagged \"FadePanel\").")]
    public Image fadePanel;
    public float fadeDuration = 1f;

    private void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        // keep black immediately
        if (fadePanel != null)
        {
            var c = fadePanel.color;
            c.a = 1f;
            fadePanel.color = c;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Try to find the FadePanel in the newly loaded scene (if our reference is null)
        if (fadePanel == null)
        {
            var go = GameObject.FindWithTag("FadePanel");
            if (go != null)
            {
                fadePanel = go.GetComponent<Image>();
                Debug.Log("[SceneFader] Found FadePanel in new scene.");
                // make sure it starts fully opaque
                var c = fadePanel.color;
                c.a = 1f;
                fadePanel.color = c;
            }
            else
            {
                Debug.LogWarning("[SceneFader] No FadePanel with tag found in scene.");
            }
        }
    }

    public IEnumerator FadeOutRoutine()
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("[SceneFader] FadePanel is null in FadeOutRoutine.");
            yield break;
        }

        TopDownMovement.isFading = true;

        float elapsed = 0f;
        var color = fadePanel.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }
        color.a = 1f;
        fadePanel.color = color;
    }

    public IEnumerator FadeIn()
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("[SceneFader] FadePanel is null in FadeIn.");
            yield break;
        }

        float elapsed = 0f;
        var color = fadePanel.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }
        color.a = 0f;
        fadePanel.color = color;

        TopDownMovement.isFading = false;
    }

    public void FadeOutAndLoad(string sceneName)
    {
        StartCoroutine(FadeOutAndLoadRoutine(sceneName));
    }

    private IEnumerator FadeOutAndLoadRoutine(string sceneName)
    {
        yield return FadeOutRoutine();
        SceneManager.LoadScene(sceneName);
    }
}
