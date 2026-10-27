using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-10000)]
public class CutsceneScene : MonoBehaviour
{
    public static bool IsActive { get; private set; }

    private readonly Dictionary<Graphic, bool> hiddenGraphics =
        new Dictionary<Graphic, bool>();

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        IsActive = false;
    }

    [SerializeField] private Image cutsceneFadePanel;

    private Image previousFadePanel;
    private SceneFader fader;

    private void Awake()
    {
        IsActive = true;

        GameUIController.ForceUnpause();

        PersistBetweenScenes[] persistentObjects = Object.FindObjectsByType<PersistBetweenScenes>(FindObjectsInactive.Include);

        foreach (PersistBetweenScenes persistent in persistentObjects)
        {
            // Index 0 is your persistent gameplay Canvas.
            // Ignore objects belonging to this cutscene itself.
            if (persistent.objectIndex != 0 ||
                persistent.gameObject.scene == gameObject.scene)
                continue;

            foreach (Graphic graphic in
                persistent.GetComponentsInChildren<Graphic>(true))
            {
                bool isFadePanel =
                    graphic.CompareTag("FadePanel") ||
                    (SceneFader.instance != null &&
                     graphic == SceneFader.instance.fadePanel);

                if (isFadePanel)
                    continue;

                hiddenGraphics[graphic] = graphic.enabled;
                graphic.enabled = false;
            }
        }
    }

    private void Start()
    {
        fader = SceneFader.instance;

        if (fader == null || cutsceneFadePanel == null)
            return;

        previousFadePanel = fader.fadePanel;

        // Make the old gameplay fade panel transparent.
        if (previousFadePanel != null &&
            previousFadePanel != cutsceneFadePanel)
        {
            Color color = previousFadePanel.color;
            color.a = 0f;
            previousFadePanel.color = color;
        }

        fader.fadePanel = cutsceneFadePanel;

        Color cutsceneColor = cutsceneFadePanel.color;
        cutsceneColor.a = 1f;
        cutsceneFadePanel.color = cutsceneColor;
    }

    private void OnDestroy()
    {
        if (fader != null && fader.fadePanel == cutsceneFadePanel)
        {
            fader.fadePanel = previousFadePanel;

            // Keep the next gameplay scene covered until its entrance fade.
            if (previousFadePanel != null)
            {
                Color color = previousFadePanel.color;
                color.a = 1f;
                previousFadePanel.color = color;
            }
        }
        
        foreach (KeyValuePair<Graphic, bool> entry in hiddenGraphics)
        {
            if (entry.Key != null)
                entry.Key.enabled = entry.Value;
        }

        hiddenGraphics.Clear();
        IsActive = false;
    }
}