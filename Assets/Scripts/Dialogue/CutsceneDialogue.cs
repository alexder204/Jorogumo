using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/*
- PURPOSE
Plays a scene-local CG cutscene: changes images at selected dialogue lines,
reveals dialogue one character at a time, and optionally loads the next scene.

- SETUP
This component sits on the cutscene GameManager, outside any UI object it hides. Make sure cutscene GameManager doesn't have DontDestroyOnLoad enabled.
Assign the cutscene Canvas, dialogue box, speaker/body texts and Scenes parent.
Put an Image component on each direct child of Scenes. Configure dialogueLines in playback order and cgEvents using zero-based dialogue indices.
Set nextSceneName when loadSceneWhenFinished is enabled.

- CONTROLS
advanceKey (Space by default): finish typing, or advance a completed line.
toggleDialogueKey (Left Shift by default): hide/show the dialogue box when allowed.
CG-only lines still require the advance key after their image transition finishes.

- INTEGRATION
This script controls CG transitions, not the full-screen entrance fade.
CutsceneScene and PlayerSpawnManager handle the separate cutscene-entry setup.
TopDownMovement.isInDialogue blocks gameplay while this cutscene is active.

- CURRENT BEHAVIOUR RETAINED
Player lookup uses the active object named Player, not the Player tag.
UnlockPlayer restores fixed speeds of 6 and 14. It does not remember prior values.
Typing and CG fades use scaled time. Dialogue data and text references must be valid.
*/

public class SceneCGDialogueManager : MonoBehaviour
{
    // One Inspector-editable dialogue entry. Array order determines playback order.
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;

        [TextArea(3, 10)]
        public string dialogueWords;

        [Tooltip("If false, this dialogue line will not show the dialogue box at all.")]
        public bool showDialogueBox = true;

        [Tooltip("If false, the player cannot hide/show the dialogue box during this line.")]
        public bool allowDialogueToggle = true;
    }

    // Associates an existing UI image (Scene CGs) with the line that first introduces it. Make dialogueIndex = dialogueLine it appears at.
    [System.Serializable]
    public class CGEvent
    {
        [Tooltip("Which dialogue line should trigger this CG? First line is 0.")]
        public int dialogueIndex;

        [Tooltip("Drag Scene1, Scene2, Scene3, etc. from Canvas > Scenes here.")]
        public Image sceneImage;
    }

    // Scene-local UI references, assigned in the Inspector.
    [Header("UI References")]
    [SerializeField] private GameObject cutsceneCanvas; // The canvas where everything sits. DontDestroyOnLoad is not enabled.

    [Tooltip("The dialogue box UI object (Could be a GameOject where everything sits) that contains SpeakerText and DialogueText.")]
    [SerializeField] private GameObject dialogueCanvas;

    [Tooltip("Empty parent object that contains Scene1, Scene2, Scene3, etc.")]
    [SerializeField] private Transform scenesParent;

    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    // The line array is required. All event indices refer to positions in this array.
    [Header("Dialogue Data")]
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("CG Events")]
    [SerializeField] private CGEvent[] cgEvents;

    // Disabling automatic loading ends playback in the current scene.
    [Header("Scene Flow")]
    [SerializeField] private bool loadSceneWhenFinished = true;
    [SerializeField] private string nextSceneName;

    // Both values are durations in seconds.
    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.02f; // delay per character
    [SerializeField] private float cgFadeSpeed = 1.2f; // full CG fade duration.

    [Header("Input")]
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;
    [SerializeField] private KeyCode toggleDialogueKey = KeyCode.LeftShift;

    private TopDownMovement playerMovement;
    private Coroutine typingRoutine;
    private Coroutine cgFadeRoutine;
    private Coroutine displayRoutine;

    private Image currentSceneImage;

    private int step;
    private bool cutsceneActive;
    private bool canContinueText;
    private bool isTyping;
    private bool endingCutscene;
    private bool dialogueCanvasVisible = true;

    // Initialize the existing scene UI and start playback automatically.
    // The manager must remain active while its dialogue box is hidden.
    private void Start()
    {
        FindPlayerMovement();

        if (cutsceneCanvas != null)
            cutsceneCanvas.SetActive(true);

        SetDialogueCanvasVisible(false, true);
        PrepareSceneImages();
        StartCoroutine(StartCutscene());
    }

    // Handle cutscene controls only while playback is active and menus are closed. 
    // A press during typing reveals the full line, then next advances.
    private void Update()
    {
        if (!cutsceneActive || endingCutscene || GameUIController.isGamePaused || JournalManager.IsJournalOpen)
            return;

        if (Input.GetKeyDown(toggleDialogueKey))
        {
            ToggleDialogueCanvas();
            return;
        }

        if (Input.GetKeyDown(advanceKey))
        {
            if (isTyping)
            {
                FinishTyping();
            }
            else if (canContinueText)
            {
                AdvanceDialogue();
            }
        }
    }

    // Safely return the current data entry for input and visibility checks.
    // Other playback methods assume the configured dialogue array is valid.
    private DialogueLine GetCurrentLine()
    {
        if (dialogueLines == null || step < 0 || step >= dialogueLines.Length)
            return null;

        return dialogueLines[step];
    }

    // Toggle only lines that both display a box and allow the player to hide it.
    // This changes UI visibility. It does not pause typing or CG playback.
    private void ToggleDialogueCanvas()
    {
        DialogueLine currentLine = GetCurrentLine();

        if (currentLine == null || !currentLine.showDialogueBox || !currentLine.allowDialogueToggle)
            return;

        SetDialogueCanvasVisible(!dialogueCanvasVisible, true);
    }

    // Show/hide the dialogue object, optionally recording its toggle state.
    // updateState = false hides a CG-only line without changing the remembered state.
    private void SetDialogueCanvasVisible(bool visible, bool updateState = true)
    {
        if (updateState)
            dialogueCanvasVisible = visible;

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(visible);
    }

    // Find an optional movement component on the active object named Player.
    // A cutscene placeholder without TopDownMovement is allowed. The global lock still applies.
    private void FindPlayerMovement()
    {
        GameObject player = GameObject.Find("Player");

        if (player != null)
            playerMovement = player.GetComponent<TopDownMovement>();
    }

    // Reset each direct child Image to transparent before playing any CG events.
    // Objects stay active so later transitions can reveal them by changing alpha.
    private void PrepareSceneImages()
    {
        if (scenesParent == null)
            return;

        for (int i = 0; i < scenesParent.childCount; i++)
        {
            Image image = scenesParent.GetChild(i).GetComponent<Image>();

            if (image == null)
                continue;

            image.gameObject.SetActive(true);
            SetImageAlpha(image, 0f);
        }
    }

    // Reset playback flags, lock gameplay and begin the first dialogue entry.
    // This coroutine schedules line playback and then completes immediately.
    private IEnumerator StartCutscene()
    {
        step = 0;
        cutsceneActive = true;
        canContinueText = false;
        isTyping = false;
        endingCutscene = false;

        LockPlayer();

        if (cutsceneCanvas != null)
            cutsceneCanvas.SetActive(true);

        displayRoutine = StartCoroutine(DisplayCurrentDialogue());
        yield break;
    }

    // Prepare one entry: clear old text, set box visibility, apply its CG event, then type.
    // Advancing is disabled until the CG fade and any typing have completed.
    private IEnumerator DisplayCurrentDialogue()
    {
        if (step >= dialogueLines.Length)
        {
            StartCoroutine(EndCutscene());
            yield break;
        }

        DialogueLine line = dialogueLines[step];

        canContinueText = false;
        isTyping = false;

        ClearDialogueText();

        if (!line.showDialogueBox)
        {
            SetDialogueCanvasVisible(false, false);
        }
        else
        {
            SetDialogueCanvasVisible(true, true);
        }

        bool cgChanged = ApplyCGEvent(step);

        if (cgChanged && cgFadeRoutine != null)
            yield return cgFadeRoutine;

        if (!line.showDialogueBox)
        {
            isTyping = false;
            canContinueText = true;
            yield break;
        }

        SetText(speakerText, line.speaker);
        StopRoutine(ref typingRoutine);

        typingRoutine = StartCoroutine(Typing(line.dialogueWords));
    }

    // Find the first event matching this dialogue index. If two are the same event back-to-back, it ignores it.
    private bool ApplyCGEvent(int dialogueIndex)
    {
        if (cgEvents == null)
            return false;

        foreach (CGEvent cgEvent in cgEvents)
        {
            if (cgEvent.dialogueIndex == dialogueIndex)
            {
                return ChangeSceneImage(cgEvent.sceneImage);
            }
        }

        return false;
    }

    // Start a new CG fade only when a different valid Image is requested.
    // The return value tells line playback whether it needs to wait for a transition.
    private bool ChangeSceneImage(Image nextImage)
    {
        if (nextImage == null || currentSceneImage == nextImage)
            return false;

        StopRoutine(ref cgFadeRoutine);

        cgFadeRoutine = StartCoroutine(FadeSceneImages(currentSceneImage, nextImage));
        return true;
    }

    // Fade the first CG in. For later CGs, reveal the new image beneath the old one.
    // This is an image transition within the cutscene, independent of SceneFader.
    private IEnumerator FadeSceneImages(Image oldImage, Image newImage)
    {
        newImage.gameObject.SetActive(true);

        if (oldImage == null)
        {
            SetImageAlpha(newImage, 0f);

            float firstTimer = 0f;

            while (firstTimer < cgFadeSpeed)
            {
                firstTimer += Time.deltaTime;
                float t = Mathf.Clamp01(firstTimer / cgFadeSpeed);

                SetImageAlpha(newImage, Mathf.Lerp(0f, 1f, t));

                yield return null;
            }

            SetImageAlpha(newImage, 1f);

            currentSceneImage = newImage;
            cgFadeRoutine = null;
            yield break;
        }

        newImage.transform.SetSiblingIndex(oldImage.transform.GetSiblingIndex());

        SetImageAlpha(newImage, 1f);

        float timer = 0f;
        float oldStartAlpha = oldImage.color.a;

        while (timer < cgFadeSpeed)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / cgFadeSpeed);

            SetImageAlpha(oldImage, Mathf.Lerp(oldStartAlpha, 0f, t));

            yield return null;
        }

        SetImageAlpha(oldImage, 0f);
        SetImageAlpha(newImage, 1f);

        currentSceneImage = newImage;
        cgFadeRoutine = null;
    }

    // Append dialogue characters with a delay while adding rich-text tags without delays.
    // The angle-bracket tracking preserves TMP markup such as <b> while text appears.
    private IEnumerator Typing(string line)
    {
        isTyping = true;
        canContinueText = false;

        SetText(dialogueText, "");

        bool addingRichTextTag = false;

        foreach (char letter in line)
        {
            if (dialogueText == null)
                yield break;

            if (letter == '<' || addingRichTextTag)
            {
                addingRichTextTag = true;
                dialogueText.text += letter;

                if (letter == '>')
                    addingRichTextTag = false;
            }
            else
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        typingRoutine = null;
        isTyping = false;
        canContinueText = true;
    }

    // Cancel the reveal coroutine and show the complete current dialogue immediately.
    // This completes the current line only. It does not advance to the next one.
    private void FinishTyping()
    {
        DialogueLine currentLine = GetCurrentLine();

        if (currentLine == null || !currentLine.showDialogueBox)
            return;

        StopRoutine(ref typingRoutine);
        SetText(dialogueText, currentLine.dialogueWords);

        isTyping = false;
        canContinueText = true;
    }

    // Move to the next data entry, or end playback after the last entry.
    // Cancel the previous display routine before preparing another line.
    private void AdvanceDialogue()
    {
        step++;

        if (step >= dialogueLines.Length)
        {
            StartCoroutine(EndCutscene());
            return;
        }

        StopRoutine(ref displayRoutine);

        displayRoutine = StartCoroutine(DisplayCurrentDialogue());
    }

    // Stop ongoing work, hide the dialogue box and release the movement lock.
    // When configured, ask the persistent fader to load the next scene, otherwise load directly.
    private IEnumerator EndCutscene()
    {
        if (endingCutscene)
            yield break;

        endingCutscene = true;
        cutsceneActive = false;
        canContinueText = false;
        isTyping = false;

        StopRoutine(ref displayRoutine);
        StopRoutine(ref typingRoutine);
        StopRoutine(ref cgFadeRoutine);

        SetDialogueCanvasVisible(false, true);

        UnlockPlayer();

        if (loadSceneWhenFinished && !string.IsNullOrEmpty(nextSceneName))
        {
            if (SceneFader.instance != null)
            {
                SceneFader.instance.FadeOutAndLoad(nextSceneName);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }

        yield break;
    }

    // This locks the player in place by putting move and sprint speed to 0.
    private void LockPlayer()
    {
        TopDownMovement.isInDialogue = true;

        if (playerMovement != null)
        {
            playerMovement.moveSpeed = 0f;
            playerMovement.sprintSpeed = 0f;
        }
    }

    // This unlocks the player by putting move and sprint speed back up to default values (6 and 14).
    private void UnlockPlayer()
    {
        TopDownMovement.isInDialogue = false;

        if (playerMovement != null)
        {
            playerMovement.moveSpeed = 6f;
            playerMovement.sprintSpeed = 14f;
        }
    }

    // Stop the coroutine and clear its original reference.
    private void StopRoutine(ref Coroutine routine)
    {
        if (routine == null)
            return;

        StopCoroutine(routine);
        routine = null;
    }

    // Assign text only when the referenced text component exists.
    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }

    // Clear both fields before displaying another dialogue line.
    private void ClearDialogueText()
    {
        SetText(speakerText, "");
        SetText(dialogueText, "");
    }

    // Change transparency while preserving the image's RGB color.
    private static void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}