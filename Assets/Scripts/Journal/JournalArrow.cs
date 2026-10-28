using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class JournalArrow : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Animator animator;
    [SerializeField] private Graphic arrowGraphic;
    [SerializeField] private Graphic arrowStartGraphic;
    [SerializeField] private string activeState;
    [SerializeField] private string idleState;
    [SerializeField] private Color enabledColor = Color.white;
    [SerializeField] private Color disabledColor = new Color(1f, 1f, 1f, 0.35f);

    private Button button;
    private bool hovered;
    private bool available;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (animator != null)
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    public void SetAvailable(bool value)
    {
        if (button == null)
            button = GetComponent<Button>();

        available = value;
        button.interactable = value;

        if (arrowGraphic != null)
            arrowGraphic.color = value ? enabledColor : disabledColor;

        if (arrowStartGraphic != null)
            arrowStartGraphic.color = value ? enabledColor : disabledColor;

        UpdateAnimation();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        UpdateAnimation();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        UpdateAnimation();
    }

    private void OnEnable()
    {
        hovered = false;
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (animator == null || !animator.isActiveAndEnabled)
            return;

        string state = available && hovered ? activeState : idleState;

        if (!string.IsNullOrEmpty(state))
            animator.Play(state, 0, 0f);
    }
}