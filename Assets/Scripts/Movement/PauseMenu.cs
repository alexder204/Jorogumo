using UnityEngine;
using UnityEngine.UI;  // Add this for interacting with UI elements (like buttons)
using System.Collections;
using System.Collections.Generic;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject areYouSure;  // Reference to the AreYouSure GameObject
    private AudioSource[] allAudioSources; // Array to store all audio sources

    public static bool isGamePaused = false;

    private bool isPaused = false;
    private bool isConfirmingQuit = false;  // To track whether the user is confirming quit

    void Start()
    {
        areYouSure.SetActive(false);  // Make sure the AreYouSure panel is hidden initially
    }

    void Update()
    {
        // Only allow pausing if not in dialogue
        if (Input.GetKeyDown(KeyCode.Escape) && !TopDownMovement.isInDialogue)
        {
            if (isConfirmingQuit)
            {
                // If the user is confirming quit, do nothing
                return;
            }

            if (settingsMenu.activeSelf)
            {
                BackToPauseMenu();
            }
            else
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        if (isConfirmingQuit) return; // Don't toggle pause if confirming quit

        isPaused = !isPaused;
        isGamePaused = isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            pauseMenu.SetActive(true);
            settingsMenu.SetActive(false);

            // Pause all audio sources
            PauseAllAudio();
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenu.SetActive(false);
            settingsMenu.SetActive(false);

            // Unpause all audio sources
            UnpauseAllAudio();
        }
    }

    public void OpenSettings()
    {
        StartCoroutine(OpenSettingsDelay(0.25f));
    }
    private IEnumerator OpenSettingsDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        StartCoroutine(CloseSettingsDelay(0.25f));
    }

    private IEnumerator CloseSettingsDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void OnQuitButtonClicked()
    {
        if (!isConfirmingQuit)
        {
            // Display the "Are you sure?" confirmation dialog
            StartCoroutine(QuitButtonClickedDelay(0.25f));
            isConfirmingQuit = true;

            // Hide the pause menu to prevent access while confirming
            DisablePauseMenuInteractions();  // Disable the interactions in the pause menu
        }
    }

    private IEnumerator QuitButtonClickedDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        areYouSure.SetActive(true);
    }

    public void OnYesQuitClicked()
    {
        Debug.Log("Game is quitting...");
        StartCoroutine(YesQuitButtonClickedDelay(0.25f));
    }

    private IEnumerator YesQuitButtonClickedDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public void OnNoQuitClicked()
    {
        isConfirmingQuit = false;
        StartCoroutine(NoQuitButtonClickedDelay(0.25f));
    }

    private IEnumerator NoQuitButtonClickedDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        areYouSure.SetActive(false);

        EnablePauseMenuInteractions();  // Enable the interactions in the pause menu
        pauseMenu.SetActive(true);
    }

    private void PauseAllAudio()
    {
        var sources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audioSource in sources)
        {
            if (audioSource != null && audioSource.isActiveAndEnabled)
                audioSource.Pause();
        }
    }

    private void UnpauseAllAudio()
    {
        var sources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audioSource in sources)
        {
            if (audioSource != null && audioSource.isActiveAndEnabled)
                audioSource.UnPause();
        }
    }


    // Disable interactions in the pause menu (e.g., buttons)
    private void DisablePauseMenuInteractions()
    {
        Button[] buttons = pauseMenu.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.interactable = false;
        }
    }

    // Re-enable interactions in the pause menu
    private void EnablePauseMenuInteractions()
    {
        Button[] buttons = pauseMenu.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.interactable = true;
        }
    }
}
