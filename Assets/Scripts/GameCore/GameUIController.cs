using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/*
 * PURPOSE:
 * Controls pause, inventory, journal, settings, and quit confirmation.
 * Handles menu shortcuts and pauses gameplay while a screen is open.
 *
 * IMPORTANT VARIABLES:
 * currentScreen   - The currently open menu.
 * pauseMenu       - Pause panel.
 * settingsMenu    - Settings panel.
 * areYouSure      - Quit confirmation panel.
 * inventoryUI     - Component that draws inventory items.
 * journalManager  - Component that stores and displays notes.
 * pausedSources   - Audio sources paused when opening a menu.
 *
 * SETUP:
 * Attach to an always-active object outside the menu panels.
 * Assign all references in the Inspector.
 * Keep only one PauseManager active.
 */

public class GameUIController : MonoBehaviour
{
    public enum MenuScreen
    {
        None,
        Pause,
        Inventory,
        Journal,
        Settings,
        QuitConfirmation,
        NoteReading
    }

    public static GameUIController Instance { get; private set; }

    [Header("Pause Panels")]
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject areYouSure;

    [Header("Inventory and Journal")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private JournalManager journalManager;

    private static MenuScreen currentScreen = MenuScreen.None;
    private static int blockedInputFrame = -1;

    private readonly List<AudioSource> pausedSources = new List<AudioSource>();

    private float previousTimeScale = 1f;

    public static MenuScreen CurrentScreen => currentScreen;

    // Retain these names because other scripts already use them.
    public static bool isJournalOpen => currentScreen == MenuScreen.Journal;

    public static bool isInventoryOpen => currentScreen == MenuScreen.Inventory;

    public static bool isGamePaused
    {
        get => currentScreen != MenuScreen.None;

        // Compatibility with SaveSystem's existing reset assignment.
        set
        {
            if (!value)
            {
                ForceUnpause();
            }
            else if (Instance != null && !isGamePaused)
            {
                Instance.OpenScreen(MenuScreen.Pause);
            }
        }
    }

    public static bool IsGameplayBlocked => isGamePaused || TopDownMovement.isInDialogue || TopDownMovement.isFading || TopDownMovement.isSceneLoading || blockedInputFrame == Time.frameCount || MenuKeyPressed;

    // Prevents world input before this controller's Update runs.
    private static bool MenuKeyPressed => Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Escape) || (Input.GetKeyDown(KeyCode.J) && Instance != null && Instance.journalManager != null && Instance.journalManager.HasNotes);

    private static bool CanOpenMenu => !TopDownMovement.isInDialogue && !TopDownMovement.isFading && !TopDownMovement.isSceneLoading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
        currentScreen = MenuScreen.None;
        blockedInputFrame = -1;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Only one GameUIController should be active.");
            enabled = false;
            return;
        }

        Instance = this;
        currentScreen = MenuScreen.None;
        UpdatePanels();
    }

    private void Update()
    {
        if (CutsceneScene.IsActive)
            return;

        // Priority prevents two shortcuts opening screens together.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscape();
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleJournal();
        }
    }

    public bool OpenNoteReading()
    {
        if (currentScreen != MenuScreen.None || !CanOpenMenu)
            return false;

        OpenScreen(MenuScreen.NoteReading);

        return currentScreen == MenuScreen.NoteReading;
    }

    private void HandleEscape()
    {
        switch (currentScreen)
        {
            case MenuScreen.None:
                OpenScreen(MenuScreen.Pause);
                break;

            case MenuScreen.Settings:
                BackToPauseMenu();
                break;

            case MenuScreen.QuitConfirmation:
                // Require the player to choose Yes or No.
                break;

            case MenuScreen.NoteReading:
                if (journalManager != null)
                    journalManager.ClosePickedUpNote();
                else
                    CloseMenus();
                break;

            default:
                CloseMenus();
                break;
        }
    }

    public void TogglePause()
    {
        ToggleScreen(MenuScreen.Pause);
    }

    public void ToggleInventory()
    {
        ToggleScreen(MenuScreen.Inventory);
    }

    public void ToggleJournal()
    {
        ToggleScreen(MenuScreen.Journal);
    }

    private void ToggleScreen(MenuScreen screen)
    {
        if (currentScreen == screen)
        {
            CloseMenus();
        }
        else if (currentScreen == MenuScreen.None)
        {
            OpenScreen(screen);
        }
    }

    private void OpenScreen(MenuScreen screen)
    {
        if (currentScreen != MenuScreen.None || !CanOpenMenu)
            return;

        // Guard the opening path, including keyboard and UI button access.
        if (screen == MenuScreen.Journal && (journalManager == null || !journalManager.HasNotes))
            return;

        if (!HasPanel(screen))
        {
            Debug.LogWarning("Missing panel reference for " + screen);
            return;
        }

        previousTimeScale = Time.timeScale;
        currentScreen = screen;
        BlockGameplayForThisFrame();

        Time.timeScale = 0f;
        PauseAllAudio();
        UpdatePanels();
    }

    private bool HasPanel(MenuScreen screen)
    {
        switch (screen)
        {
            case MenuScreen.Pause:
                return pauseMenu != null;

            case MenuScreen.Inventory:
                return inventoryUI != null && inventoryUI.inventoryUI != null;

            case MenuScreen.Journal:
                return journalManager != null && journalManager.journalUI != null;

            case MenuScreen.NoteReading:
                return journalManager != null && journalManager.CanDisplayPickedUpNote;

            default:
                return false;
        }
    }

    public void CloseMenus()
    {
        if (currentScreen == MenuScreen.None)
            return;

        bool wasReadingNote = currentScreen == MenuScreen.NoteReading;

        currentScreen = MenuScreen.None;
        BlockGameplayForThisFrame();

        UpdatePanels();
        Time.timeScale = previousTimeScale;
        UnpauseAllAudio();

        // General resets, including loading a save, cancel the reaction.
        // ClosePickedUpNote captures it before calling this method.
        if (wasReadingNote && journalManager != null)
            journalManager.CancelPickedUpNote();
    }

    public void OpenSettings()
    {
        if (currentScreen != MenuScreen.Pause || settingsMenu == null)
            return;

        currentScreen = MenuScreen.Settings;
        UpdatePanels();
    }

    public void BackToPauseMenu()
    {
        if (currentScreen != MenuScreen.Settings)
            return;

        currentScreen = MenuScreen.Pause;
        UpdatePanels();
    }

    public void OnQuitButtonClicked()
    {
        if (currentScreen != MenuScreen.Pause || areYouSure == null)
            return;

        currentScreen = MenuScreen.QuitConfirmation;
        UpdatePanels();
    }

    public void OnNoQuitClicked()
    {
        if (currentScreen != MenuScreen.QuitConfirmation)
            return;

        currentScreen = MenuScreen.Pause;
        UpdatePanels();
    }

    public void OnYesQuitClicked()
    {
        if (currentScreen != MenuScreen.QuitConfirmation)
            return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void UpdatePanels()
    {
        // Leave pause visible behind the quit confirmation.
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(currentScreen == MenuScreen.Pause || currentScreen == MenuScreen.QuitConfirmation);

            Button[] buttons = pauseMenu.GetComponentsInChildren<Button>(true);

            foreach (Button button in buttons)
            {
                // Do not disable the confirmation's own buttons.
                if (areYouSure != null && button.transform.IsChildOf(areYouSure.transform))
                    continue;

                button.interactable = currentScreen == MenuScreen.Pause;
            }
        }

        if (settingsMenu != null)
            settingsMenu.SetActive(currentScreen == MenuScreen.Settings);

        if (areYouSure != null)
            areYouSure.SetActive(currentScreen == MenuScreen.QuitConfirmation);

        if (inventoryUI != null)
            inventoryUI.SetVisible(currentScreen == MenuScreen.Inventory);

        if (journalManager != null)
        {
            journalManager.SetVisible(currentScreen == MenuScreen.Journal);
            journalManager.SetNoteReadingVisible(currentScreen == MenuScreen.NoteReading);
        }
    }

    private void PauseAllAudio()
    {
        pausedSources.Clear();

        AudioSource[] sources = Object.FindObjectsByType<AudioSource>();

        foreach (AudioSource source in sources)
        {
            if (source != null && source.isActiveAndEnabled && source.isPlaying && !source.ignoreListenerPause)
            {
                source.Pause();
                pausedSources.Add(source);
            }
        }
    }

    private void UnpauseAllAudio()
    {
        // Resume only sources that this controller paused.
        foreach (AudioSource source in pausedSources)
        {
            if (source != null && source.isActiveAndEnabled)
                source.UnPause();
        }

        pausedSources.Clear();
    }

    public static void BlockGameplayForThisFrame()
    {
        blockedInputFrame = Time.frameCount;
    }

    public static void ForceUnpause()
    {
        if (Instance != null)
        {
            Instance.CloseMenus();
            Instance.UpdatePanels();
        }

        currentScreen = MenuScreen.None;
        Time.timeScale = 1f;
        BlockGameplayForThisFrame();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        if (isGamePaused)
        {
            Time.timeScale = previousTimeScale;
            UnpauseAllAudio();
        }

        currentScreen = MenuScreen.None;
        Instance = null;
    }
}