using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SceneCGDialogueManager : MonoBehaviour
{
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

    [System.Serializable]
    public class CGEvent
    {
        [Tooltip("Which dialogue line should trigger this CG? First line is 0.")]
        public int dialogueIndex;

        [Tooltip("Drag Scene1, Scene2, Scene3, etc. from Canvas > Scenes here.")]
        public Image sceneImage;
    }

    [Header("UI References")]
    [SerializeField] private GameObject cutsceneCanvas;

    [Tooltip("The dialogue box UI object that contains SpeakerText and DialogueText.")]
    [SerializeField] private GameObject dialogueCanvas;

    [Tooltip("Empty parent object that contains Scene1, Scene2, Scene3, etc.")]
    [SerializeField] private Transform scenesParent;

    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Dialogue Data")]
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("CG Events")]
    [SerializeField] private CGEvent[] cgEvents;

    [Header("Scene Flow")]
    [SerializeField] private bool loadSceneWhenFinished = true;
    [SerializeField] private string nextSceneName;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.02f;
    [SerializeField] private float cgFadeSpeed = 1.2f;

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

    private void Start()
    {
        FindPlayerMovement();

        if (cutsceneCanvas != null)
            cutsceneCanvas.SetActive(true);

        SetDialogueCanvasVisible(false, true);

        PrepareSceneImages();

        StartCoroutine(StartCutscene());
    }

    private void Update()
    {
        if (!cutsceneActive || endingCutscene)
            return;

        if (PauseManager.isGamePaused || JournalManager.IsJournalOpen)
            return;

        DialogueLine currentLine = GetCurrentLine();

        if (Input.GetKeyDown(toggleDialogueKey))
        {
            if (currentLine != null &&
                currentLine.showDialogueBox &&
                currentLine.allowDialogueToggle)
            {
                ToggleDialogueCanvas();
            }

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

    private DialogueLine GetCurrentLine()
    {
        if (dialogueLines == null)
            return null;

        if (step < 0 || step >= dialogueLines.Length)
            return null;

        return dialogueLines[step];
    }

    private void ToggleDialogueCanvas()
    {
        DialogueLine currentLine = GetCurrentLine();

        if (currentLine == null)
            return;

        if (!currentLine.showDialogueBox || !currentLine.allowDialogueToggle)
            return;

        SetDialogueCanvasVisible(!dialogueCanvasVisible, true);
    }

    private void SetDialogueCanvasVisible(bool visible, bool updateState = true)
    {
        if (updateState)
            dialogueCanvasVisible = visible;

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(visible);
    }

    private void FindPlayerMovement()
    {
        GameObject player = GameObject.Find("Player");

        if (player != null)
            playerMovement = player.GetComponent<TopDownMovement>();
    }

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

            Color color = image.color;
            color.a = 0f;
            image.color = color;
        }
    }

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

        if (speakerText != null)
            speakerText.text = "";

        if (dialogueText != null)
            dialogueText.text = "";

        // Handle dialogue box visibility immediately, BEFORE the CG fade.
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

        if (speakerText != null)
            speakerText.text = line.speaker;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(Typing(line.dialogueWords));
    }

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

    private bool ChangeSceneImage(Image nextImage)
    {
        if (nextImage == null)
            return false;

        if (currentSceneImage == nextImage)
            return false;

        if (cgFadeRoutine != null)
            StopCoroutine(cgFadeRoutine);

        cgFadeRoutine = StartCoroutine(FadeSceneImages(currentSceneImage, nextImage));
        return true;
    }

    private IEnumerator FadeSceneImages(Image oldImage, Image newImage)
    {
        newImage.gameObject.SetActive(true);

        // First CG: fade it in from transparent.
        if (oldImage == null)
        {
            Color firstColor = newImage.color;
            firstColor.a = 0f;
            newImage.color = firstColor;

            float firstTimer = 0f;

            while (firstTimer < cgFadeSpeed)
            {
                firstTimer += Time.deltaTime;
                float t = Mathf.Clamp01(firstTimer / cgFadeSpeed);

                Color c = newImage.color;
                c.a = Mathf.Lerp(0f, 1f, t);
                newImage.color = c;

                yield return null;
            }

            firstColor = newImage.color;
            firstColor.a = 1f;
            newImage.color = firstColor;

            currentSceneImage = newImage;
            cgFadeRoutine = null;
            yield break;
        }

        // Put the new image behind the old image.
        newImage.transform.SetSiblingIndex(oldImage.transform.GetSiblingIndex());

        // New image stays fully visible behind the old image.
        Color newColor = newImage.color;
        newColor.a = 1f;
        newImage.color = newColor;

        // Fade only the old image out.
        float timer = 0f;
        float oldStartAlpha = oldImage.color.a;

        while (timer < cgFadeSpeed)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / cgFadeSpeed);

            Color oldColor = oldImage.color;
            oldColor.a = Mathf.Lerp(oldStartAlpha, 0f, t);
            oldImage.color = oldColor;

            yield return null;
        }

        Color finalOldColor = oldImage.color;
        finalOldColor.a = 0f;
        oldImage.color = finalOldColor;

        Color finalNewColor = newImage.color;
        finalNewColor.a = 1f;
        newImage.color = finalNewColor;

        currentSceneImage = newImage;
        cgFadeRoutine = null;
    }

    private IEnumerator Typing(string line)
    {
        isTyping = true;
        canContinueText = false;

        if (dialogueText != null)
            dialogueText.text = "";

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

    private void FinishTyping()
    {
        DialogueLine currentLine = GetCurrentLine();

        if (currentLine == null)
            return;

        if (!currentLine.showDialogueBox)
            return;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        if (dialogueText != null)
            dialogueText.text = currentLine.dialogueWords;

        typingRoutine = null;
        isTyping = false;
        canContinueText = true;
    }

    private void AdvanceDialogue()
    {
        step++;

        if (step >= dialogueLines.Length)
        {
            StartCoroutine(EndCutscene());
            return;
        }

        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        displayRoutine = StartCoroutine(DisplayCurrentDialogue());
    }

    private IEnumerator EndCutscene()
    {
        if (endingCutscene)
            yield break;

        endingCutscene = true;
        cutsceneActive = false;
        canContinueText = false;
        isTyping = false;

        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
            displayRoutine = null;
        }

        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (cgFadeRoutine != null)
        {
            StopCoroutine(cgFadeRoutine);
            cgFadeRoutine = null;
        }

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

    private void LockPlayer()
    {
        TopDownMovement.isInDialogue = true;

        if (playerMovement != null)
        {
            playerMovement.moveSpeed = 0f;
            playerMovement.sprintSpeed = 0f;
        }
    }

    private void UnlockPlayer()
    {
        TopDownMovement.isInDialogue = false;

        if (playerMovement != null)
        {
            playerMovement.moveSpeed = 6f;
            playerMovement.sprintSpeed = 14f;
        }
    }
}