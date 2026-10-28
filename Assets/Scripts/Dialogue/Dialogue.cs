using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/*
 * PURPOSE:
 * Handles interaction dialogue, automatic trigger dialogue,
 * and an optional inventory item reward.
 *
 * IMPORTANT VARIABLES:
 * startMode       - Interact button or automatic trigger entry.
 * speaker         - Speaker name for each dialogue line.
 * dialogueWords   - Text for each dialogue line.
 * portrait1/2     - Portrait sprites for each line.
 * typingSpeed     - Delay between visible characters.
 * initialDelay    - Delay before each line starts typing.
 * itemToPickUp    - Optional reward after completing dialogue.
 * interactPopUp   - Shared popup above the player.
 * interactItem    - Existing event UI shown during dialogue.
 * charImage1/2    - Existing character/speaker UI containers.
 * dialogueCanvas  - Shared dialogue panel.
 * dialogueID      - Completion ID used by the save system.
 * uniqueID        - Tracks the rewarded object's used state.
 * playerColliders - Player colliders currently inside the trigger.
 * step            - Current dialogue line.
 * dialogueActive  - Whether this component owns the dialogue.
 *
 * SETUP:
 * Attach to an object with a 2D trigger and DialogueID.
 * Preserve the existing DialogueID and Inspector references.
 * Assign UniqueID when awarding an item.
 * Remove the old DialogueOnEnter or DialoguePickUp component
 * after transferring its data.
 */

namespace PlayerDialogue
{
    [RequireComponent(typeof(DialogueID))]
    public class Dialogue : MonoBehaviour
    {
        public enum StartMode
        {
            Interact,
            TriggerEnter,
            Manual
        }

        [Header("Start Behaviour")]
        [SerializeField] private StartMode startMode = StartMode.Interact;

        [Header("Dialogue Data")]
        [SerializeField] private string[] speaker;
        [SerializeField] private Sprite[] portrait1;
        [SerializeField] private Sprite[] portrait2;

        [SerializeField, TextArea(3, 10)]
        private string[] dialogueWords;

        [Header("Typing")]
        [SerializeField, Min(0f)] private float typingSpeed = 0.02f;
        [SerializeField, Min(0f)] private float initialDelay = 0.5f;

        [Header("Optional Item Reward")]
        [SerializeField] private Item itemToPickUp;

        [Header("Interaction and Event UI")]
        [SerializeField] private GameObject interactItem;
        [SerializeField] private GameObject interactPopUp;

        [Header("Shared UI — Supplied by DialogueUIManager")]
        [SerializeField] private GameObject dialogueCanvas;
        [SerializeField] private GameObject charImage1;
        [SerializeField] private GameObject charImage2;
        [SerializeField] private TextMeshProUGUI speakerText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image portraitImage1;
        [SerializeField] private Image portraitImage2;

        private DialogueUIManager ui;
        private DialogueID dialogueID;
        private UniqueID uniqueID;

        private readonly HashSet<Collider2D> playerColliders = new HashSet<Collider2D>();

        private Coroutine typingRoutine;
        private int step;
        private int startedFrame;
        private bool dialogueActive;
        private bool isTyping;
        private bool pendingAutomaticStart;

        private bool PlayerInRange => playerColliders.Count > 0;

        private bool IsCompleted => ui != null && dialogueID != null && ui.HasCompletedDialogue(dialogueID);

        private void Awake()
        {
            dialogueID = GetComponent<DialogueID>();
            uniqueID = GetComponent<UniqueID>();
        }

        private void Start()
        {
            ConnectUI();

            // Restore the old lookup for the shared player popup.
            if (interactPopUp == null)
                interactPopUp = GameObject.Find("InteractPopUp");

            if (IsCompleted)
            {
                if (interactPopUp != null)
                    interactPopUp.SetActive(false);

                if (interactItem != null)
                    interactItem.SetActive(false);
            }
        }

        private bool ConnectUI()
        {
            ui = DialogueUIManager.Instance;

            if (ui == null)
                return false;

            dialogueCanvas = ui.dialogueCanvas;
            charImage1 = ui.charImage1;
            charImage2 = ui.charImage2;
            speakerText = ui.speakerText;
            dialogueText = ui.dialogueText;
            portraitImage1 = ui.portraitImage1;
            portraitImage2 = ui.portraitImage2;

            return dialogueCanvas != null && dialogueText != null;
        }

        private void Update()
        {
            if (dialogueActive)
            {
                // Dialogue must still receive its own input.
                if (GameUIController.isGamePaused || TopDownMovement.isFading || TopDownMovement.isSceneLoading)
                    return;

                if (Time.frameCount == startedFrame)
                    return;

                if (Input.GetButtonDown("Interact"))
                {
                    if (isTyping)
                        FinishTyping();
                    else
                        AdvanceDialogue();
                }

                return;
            }

            // Manual dialogue starts only through another script.
            if (startMode == StartMode.Manual)
                return;

            if (!PlayerInRange || IsCompleted || GameUIController.IsGameplayBlocked)
                return;

            if (startMode == StartMode.TriggerEnter)
            {
                if (pendingAutomaticStart)
                    TryStartDialogue();
            }
            else if (Input.GetButtonDown("Interact"))
            {
                TryStartDialogue();
            }
        }

        public void PlayNoteReaction()
        {
            TryStartDialogue(true);
        }

        private void TryStartDialogue(bool fromNoteReader = false)
        {
            if (!isActiveAndEnabled || dialogueActive)
                return;

            if (fromNoteReader)
            {
                // The note screen has just closed. Its frame input block
                // must remain active for world interactions, but the
                // reaction can start immediately.
                if (startMode != StartMode.Manual || GameUIController.isGamePaused || TopDownMovement.isInDialogue || TopDownMovement.isFading || TopDownMovement.isSceneLoading)
                    return;
            }
            else if (GameUIController.IsGameplayBlocked)
            {
                return;
            }

            if (!ConnectUI())
                return;

            if (IsCompleted)
                return;

            if (string.IsNullOrWhiteSpace(dialogueID.id))
            {
                Debug.LogError("Dialogue needs a non-empty DialogueID.", this);

                pendingAutomaticStart = false;
                return;
            }

            if (dialogueWords == null || dialogueWords.Length == 0)
            {
                Debug.LogError("Dialogue has no lines.", this);
                pendingAutomaticStart = false;
                return;
            }

            if (itemToPickUp != null && (uniqueID == null || string.IsNullOrWhiteSpace(uniqueID.id)))
            {
                Debug.LogError("An item reward needs a valid UniqueID.", this);

                pendingAutomaticStart = false;
                return;
            }

            pendingAutomaticStart = false;
            dialogueActive = true;
            startedFrame = Time.frameCount;
            step = 0;

            TopDownMovement.isInDialogue = true;

            // Original UI activation behaviour.
            dialogueCanvas.SetActive(true);

            if (interactItem != null)
                interactItem.SetActive(true);

            if (interactPopUp != null)
                interactPopUp.SetActive(true);

            if (charImage1 != null)
                charImage1.SetActive(true);

            if (charImage2 != null)
                charImage2.SetActive(true);

            DisplayLine();
        }

        private void DisplayLine()
        {
            if (speakerText != null)
            {
                speakerText.text = speaker != null && step < speaker.Length ? speaker[step]: "";
            }

            UpdatePortrait(portraitImage1, portrait1);
            UpdatePortrait(portraitImage2, portrait2);

            dialogueText.text = dialogueWords[step] ?? "";
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.ForceMeshUpdate();

            isTyping = true;
            typingRoutine = StartCoroutine(TypeLine());
        }

        private void UpdatePortrait(Image image, Sprite[] portraits)
        {
            if (image == null)
                return;

            image.sprite = portraits != null && step < portraits.Length ? portraits[step] : null;

            // Do not hide speaker UI because a sprite is missing.
            image.enabled = true;
        }

        private IEnumerator TypeLine()
        {
            if (initialDelay > 0f)
                yield return new WaitForSeconds(initialDelay);

            int characterCount = dialogueText.textInfo.characterCount;

            if (typingSpeed <= 0f)
            {
                dialogueText.maxVisibleCharacters = int.MaxValue;
            }
            else
            {
                WaitForSeconds delay = new WaitForSeconds(typingSpeed);

                for (int visible = 1; visible <= characterCount; visible++)
                {
                    dialogueText.maxVisibleCharacters = visible;
                    yield return delay;
                }
            }

            isTyping = false;
            typingRoutine = null;
        }

        private void FinishTyping()
        {
            if (typingRoutine != null)
                StopCoroutine(typingRoutine);

            typingRoutine = null;
            isTyping = false;

            if (dialogueText != null)
                dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        private void AdvanceDialogue()
        {
            if (step + 1 < dialogueWords.Length)
            {
                step++;
                DisplayLine();
            }
            else
            {
                CompleteDialogue();
            }
        }

        private void CompleteDialogue()
        {
            if (itemToPickUp != null)
            {
                if (Inventory.instance == null || !Inventory.instance.Add(itemToPickUp))
                {
                    EndDialogue();

                    if (UIManager.instance != null)
                        UIManager.instance.ShowMessage("Inventory full.");

                    // Remain incomplete so the reward can be retried.
                    return;
                }

                if (ObjectStateTracker.Instance != null)
                    ObjectStateTracker.Instance.MarkUsed(uniqueID.id);
            }

            ui.MarkDialogueComplete(dialogueID);
            EndDialogue();

            if (itemToPickUp != null)
            {
                if (UIManager.instance != null)
                {
                    UIManager.instance.ShowMessage($"Picked up {itemToPickUp.itemName}!");
                }

                gameObject.SetActive(false);
            }
        }

        private void EndDialogue()
        {
            if (!dialogueActive)
                return;

            if (typingRoutine != null)
                StopCoroutine(typingRoutine);

            typingRoutine = null;
            isTyping = false;
            dialogueActive = false;

            // Original UI closing behaviour.
            if (dialogueCanvas != null)
                dialogueCanvas.SetActive(false);

            if (interactItem != null)
                interactItem.SetActive(false);

            if (interactPopUp != null)
                interactPopUp.SetActive(false);

            if (charImage1 != null)
                charImage1.SetActive(false);

            if (charImage2 != null)
                charImage2.SetActive(false);

            if (dialogueText != null)
                dialogueText.maxVisibleCharacters = int.MaxValue;

            TopDownMovement.isInDialogue = false;
            GameUIController.BlockGameplayForThisFrame();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (startMode == StartMode.Manual)
                return;

            if (!collision.CompareTag("Player"))
                return;

            bool wasInRange = PlayerInRange;
            playerColliders.Add(collision);

            if (wasInRange)
                return;

            if (startMode == StartMode.TriggerEnter)
            {
                pendingAutomaticStart = true;
            }
            else if (!IsCompleted && interactPopUp != null)
            {
                interactPopUp.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (startMode == StartMode.Manual)
                return;

            if (!playerColliders.Remove(collision))
                return;

            if (PlayerInRange)
                return;

            pendingAutomaticStart = false;
            EndDialogue();

            if (interactPopUp != null)
                interactPopUp.SetActive(false);
        }

        private void OnDisable()
        {
            bool wasUsingPopup = PlayerInRange || dialogueActive;

            EndDialogue();
            playerColliders.Clear();
            pendingAutomaticStart = false;

            // A distant disabled object should not hide the shared popup.
            if (wasUsingPopup && interactPopUp != null)
                interactPopUp.SetActive(false);
        }
    }
}