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
        if (fadePanel == null)
        {
            Debug.Log("FadePanel is null on scene load, searching for it...");
            GameObject foundPanel = GameObject.FindWithTag("FadePanel");
            if (foundPanel != null)
            {
                fadePanel = foundPanel.GetComponent<UnityEngine.UI.Image>();
                Debug.Log("Found FadePanel in new scene.");

                if (fadePanel != null)
                {
                    Color c = fadePanel.color;
                    c.a = 1f;
                    fadePanel.color = c;
                    fadePanel.gameObject.SetActive(true);

                    Debug.Log("Starting FadeIn coroutine...");
                    StartCoroutine(FadeIn());
                }
                else
                {
                    Debug.LogWarning("Found GameObject with FadePanel tag but no Image component.");
                }
            }
            else
            {
                Debug.LogWarning("No GameObject with FadePanel tag found in the scene.");
            }
        }
        else
        {
            Debug.Log("FadePanel already assigned.");
            // Just in case, restart FadeIn if panel exists and alpha is 1
            if (fadePanel.color.a >= 1f)
            {
                StartCoroutine(FadeIn());
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
