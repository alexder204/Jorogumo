using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject areYouSure;
    private AudioSource[] allAudioSources;

    [SerializeField] private string newLevel;

    private bool isConfirmingQuit = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainMenu.SetActive(true);
        areYouSure.SetActive(false);
        settingsMenu.SetActive(false);
    }

    public void LoadSceneByName()
    {
        SceneManager.LoadScene(newLevel);
    }


    public void OpenSettings()
    {
        StartCoroutine(OpenSettingsDelay(0.25f));
    }
    private IEnumerator OpenSettingsDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        DisableMainMenuInteractions();
        settingsMenu.SetActive(true);
    }

    public void BackToMainMenu()
    {
        StartCoroutine(CloseSettingsDelay(0.25f));
    }

    private IEnumerator CloseSettingsDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        settingsMenu.SetActive(false);
        EnableMainMenuInteractions();
    }

    public void OnQuitButtonClicked()
    {
        if (!isConfirmingQuit)
        {
            // Display the "Are you sure?" confirmation dialog
            StartCoroutine(QuitButtonClickedDelay(0.25f));
            isConfirmingQuit = true;

            // Hide the pause menu to prevent access while confirming
            DisableMainMenuInteractions();  // Disable the interactions in the pause menu
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

        EnableMainMenuInteractions();  // Enable the interactions in the pause menu
    }

    private void DisableMainMenuInteractions()
    {
        Button[] buttons = mainMenu.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.interactable = false;
        }
    }

    private void EnableMainMenuInteractions()
    {
        Button[] buttons = mainMenu.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.interactable = true;
        }
    }
}
