using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ChipHoverController : MonoBehaviour
{
    public TextMeshProUGUI descriptionText;
    public RectTransform descriptionPanel;
    private CanvasGroup canvasGroup;

    private static ChipHoverController instance;

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false);

        canvasGroup = descriptionPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = descriptionPanel.AddComponent<CanvasGroup>();

    }

    public static void Show(string description)
    {
        if (instance == null) return;

        instance.descriptionText.text = description;
        instance.descriptionPanel.gameObject.SetActive(true);
        instance.canvasGroup.alpha = 0f;
        instance.canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad);

    }

    public static void Hide()
    {
        if (instance != null)
            instance.descriptionPanel.gameObject.SetActive(false);
    }
}