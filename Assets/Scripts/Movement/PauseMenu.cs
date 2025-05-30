using UnityEngine;
using UnityEngine.UI;  // Add this for interacting with UI elements (like buttons)

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
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void OnQuitButtonClicked()
    {
        if (!isConfirmingQuit)
        {
            // Display the "Are you sure?" confirmation dialog
            areYouSure.SetActive(true);
            isConfirmingQuit = true;

            // Hide the pause menu to prevent access while confirming
            DisablePauseMenuInteractions();  // Disable the interactions in the pause menu
        }
    }

    public void OnYesQuitClicked()
    {
        // Confirm quit and exit the game
        Debug.Log("Game is quitting...");
        Application.Quit();

        // If we're in the editor, simulate quit (doesn't work in the editor as Application.Quit() only works in the built game)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnNoQuitClicked()
    {
        // Close the "Are you sure?" dialog and show the pause menu again
        areYouSure.SetActive(false);
        isConfirmingQuit = false;

        // Re-enable the pause menu if the player cancels quitting
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
