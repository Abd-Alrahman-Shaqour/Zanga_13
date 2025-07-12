using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIImageAnimator : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float delayBeforeFade = 2f;
    [SerializeField] private bool fadeOnStart = true;

    private CanvasGroup canvasGroup;
    [SerializeField] private UIButtonAnimator targetButtonAnimator;

    private void Awake()
    {
        // Ensure a CanvasGroup exists (preferred for fading entire UI objects)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1;
    }

    private void Start()
    {
        if (fadeOnStart)
            FadeOutAfterDelay();
    }

    public void FadeOutAfterDelay()
    {
        Invoke(nameof(FadeOut), delayBeforeFade);
    }

    public void FadeOut()
    {
        canvasGroup.DOFade(0, fadeDuration).OnComplete(() =>
        {
            gameObject.SetActive(false);

            if (targetButtonAnimator != null)
            {
                targetButtonAnimator.gameObject.SetActive(true);
                targetButtonAnimator.AnimateBtnAppear();
            }
        });
    }

    public void FadeIn()
    {
        canvasGroup.alpha = 0;
        gameObject.SetActive(true);
        canvasGroup.DOFade(1, fadeDuration);
    }
}
