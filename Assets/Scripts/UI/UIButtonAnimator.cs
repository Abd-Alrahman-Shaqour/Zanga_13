using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI; // Make sure to include this namespace

[RequireComponent(typeof(Button))]
public class UIButtonAnimator : MonoBehaviour
{
    [Header("Animation params"), Space]
    [SerializeField] private bool isAnimating = false;
    public Ease ease = Ease.InOutBack;
    [Range(0.1f, 3f)] public float duration = 0.2f;

    [Header("Scale parameters"), Space]
    public Vector3 startVal = Vector3.one * 0.9f;
    [SerializeField] public Vector3 finalVal = Vector3.one;

    [Header("SFX"), Space]
    [SerializeField] private ButtonSFXType buttonSFXType;

    [Header("Event callback"), Space]
    public UnityEvent callBackFn;

    [SerializeField] Button button; // Reference to the Button component

    private void OnEnable()
    {
        isAnimating = false;
        this.transform.localScale = finalVal;

        button = GetComponent<Button>();
        if (button != null)
        {
            // Remove any existing listeners to avoid duplication
            button.onClick.RemoveListener(OnClick);
            // Add the OnClick method to the Button's OnClick event
            button.onClick.AddListener(OnClick);
        }
    }

    public virtual void OnClick()
    {
        if (isAnimating)
            return;

        isAnimating = true;

        PlayOnClickSFX();

        this.transform.DOScale(startVal, 0.1f).SetEase(Ease.InBack).OnComplete(() =>
        {
            this.transform.DOScale(finalVal, duration).SetEase(ease);
        });

        StartCoroutine(OnClickProcess(duration));
    }

    public virtual IEnumerator OnClickProcess(float timer)
    {
        yield return new WaitForSeconds(timer);
        callBackFn?.Invoke();
        isAnimating = false;
    }

    public void PlayOnClickSFX()
    {
        if (AudioManager.Instance && buttonSFXType != ButtonSFXType.none)
            AudioManager.Instance.PlaySFX(buttonSFXType.ToString());
    }

    private void OnDisable()
    {
        // Remove the OnClick listener when the object is disabled to avoid potential issues
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
        }
    }

    public void AnimateBtnAppear()
    {
        this.transform.localScale = Vector3.one * .5f;

        this.transform.DOScale(finalVal * 1.1f, duration).SetEase(Ease.InBack).OnComplete(() =>
        {
            this.transform.DOScale(finalVal, duration).SetEase(ease);
        });
    }

    public void Set_Interactability(bool _state)
    {
        button.interactable = _state;
    }

    public enum ButtonSFXType
    {
        none,
        UI_play,
        UI_select,
        
        UI_invalid,
        UI_error,

        UI_toggle_on,
        UI_toggle_off,

        UI_popup_open,
        UI_popup_close,

        UI_popup_slideIn,
        UI_popup_slideOut,

        UI_advance,
        UI_back,
        
    }
}