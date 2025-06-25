using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public static SceneFader instance;

    public Image fadePanel;
    public float fadeDuration = 1f;

    private void Awake()
    {
        // Singleton logic
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
    }

    private void Start()
    {
        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 1f;
            fadePanel.color = c;
        }

        StartCoroutine(FadeIn());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Refresh fadePanel if it's missing (like in MainMenu)
        if (fadePanel == null)
        {
            GameObject foundPanel = GameObject.FindWithTag("FadePanel");
            if (foundPanel != null)
            {
                fadePanel = foundPanel.GetComponent<Image>();
                if (fadePanel != null)
                {
                    Color c = fadePanel.color;
                    c.a = 1f;
                    fadePanel.color = c;
                    fadePanel.gameObject.SetActive(true);
                    StartCoroutine(FadeIn());
                }
            }
        }
    }

    public void FadeOutAndLoad(string sceneName)
    {
        StartCoroutine(FadeOutAndLoadRoutine(sceneName));
    }

    private IEnumerator FadeOutAndLoadRoutine(string sceneName)
    {
        if (fadePanel != null)
        {
            float elapsed = 0f;
            Color color = fadePanel.color;

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

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeIn()
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        Color color = fadePanel.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        color.a = 0f;
        fadePanel.color = color;
    }
}
