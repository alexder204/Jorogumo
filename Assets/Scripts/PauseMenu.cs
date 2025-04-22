using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    private AudioSource[] allAudioSources; // Array to store all audio sources

    public static bool isGamePaused = false;

    private bool isPaused = false;

    void Start()
    {
        // Use FindObjectsByType to get all AudioSource components
        allAudioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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

    private void PauseAllAudio()
    {
        foreach (AudioSource audioSource in allAudioSources)
        {
            audioSource.Pause(); // Pause every audio source (including music)
        }
    }

    private void UnpauseAllAudio()
    {
        foreach (AudioSource audioSource in allAudioSources)
        {
            audioSource.UnPause(); // Resume every audio source
        }
    }
}
