using UnityEngine;
using UnityEngine.UI;

public class GameScreenScaler : MonoBehaviour
{
    [SerializeField] private RectTransform gameArea;

    private const float DesignWidth = 1920f;
    private const float DesignHeight = 1080f;
    private const float TargetAspect = DesignWidth / DesignHeight;

    private RectTransform canvasRect;
    private Camera gameCamera;
    private readonly Image[] bars = new Image[4];

    private void Awake()
    {
        canvasRect = GetComponent<RectTransform>();

        // Black bars are siblings of GameArea.
        for (int i = 0; i < bars.Length; i++)
        {
            GameObject bar = new GameObject(
                "ScreenBar" + i,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            bar.transform.SetParent(transform, false);

            bars[i] = bar.GetComponent<Image>();
            bars[i].color = Color.black;

            // Prevent clicks through the unused screen area.
            bars[i].raycastTarget = true;
        }
    }

    private void LateUpdate()
    {
        if (gameArea == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        if (gameCamera == null)
            gameCamera = Camera.main;

        float screenAspect = (float)Screen.width / Screen.height;

        Rect viewport = new Rect(0f, 0f, 1f, 1f);

        if (screenAspect > TargetAspect)
        {
            viewport.width = TargetAspect / screenAspect;
            viewport.x = (1f - viewport.width) / 2f;
        }
        else
        {
            viewport.height = screenAspect / TargetAspect;
            viewport.y = (1f - viewport.height) / 2f;
        }

        if (gameCamera != null)
            gameCamera.rect = viewport;

        // Keep the UI's original coordinate system and scale uniformly.
        gameArea.anchorMin = new Vector2(0.5f, 0.5f);
        gameArea.anchorMax = new Vector2(0.5f, 0.5f);
        gameArea.pivot = new Vector2(0.5f, 0.5f);
        gameArea.anchoredPosition = Vector2.zero;
        gameArea.sizeDelta = new Vector2(DesignWidth, DesignHeight);

        float scale = Mathf.Min(
            canvasRect.rect.width / DesignWidth,
            canvasRect.rect.height / DesignHeight);

        gameArea.localScale = new Vector3(scale, scale, 1f);

        SetBar(bars[0], Vector2.zero,
            new Vector2(viewport.xMin, 1f));

        SetBar(bars[1], new Vector2(viewport.xMax, 0f),
            Vector2.one);

        SetBar(bars[2], new Vector2(viewport.xMin, 0f),
            new Vector2(viewport.xMax, viewport.yMin));

        SetBar(bars[3], new Vector2(viewport.xMin, viewport.yMax),
            new Vector2(viewport.xMax, 1f));
    }

    private void SetBar(Image bar, Vector2 min, Vector2 max)
    {
        RectTransform rect = bar.rectTransform;
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        bar.enabled = max.x > min.x && max.y > min.y;
        rect.SetAsLastSibling();
    }
}