using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using DG.Tweening;

public class UIPanelAnimator : MonoBehaviour
{
#region Vars
    [Header("Main Components"), Space]
    [SerializeField]  CanvasGroup this_CanvasGroup;
    [SerializeField]  RectTransform this_Rect;
    [SerializeField]  RectTransform popupContent_Rect;

    [Header("Animation Parameters"), Space]
    [SerializeField]  bool isAnimating = false;
    [SerializeField] public float duration = 0.25f;
    [SerializeField]  Ease easeIn_Type = Ease.InCubic;
    [SerializeField]  Ease easeOut_Type = Ease.OutCubic;
    [SerializeField]  AnimationType animationType;

    [Header("Scale Parameters"), Space]
    [SerializeField] Vector3 initScale = Vector3.one * 0.75f;
    [SerializeField]  Vector3 finalScale = Vector3.one;
    
    [Header("Slide Parameters"), Space]
    [SerializeField]  SlideFrom slideFrom;
    [SerializeField]  AnchorPreset anchorPreset;

    [Space]
    [ SerializeField]  Vector3 startPos = Vector3.one;
    [ SerializeField]  Vector3 endPos = Vector3.one;
#endregion --- Vars ---

    void OnEnable()
    {
        InitPanelProperties();
        AnimateOpen();
    }

    void InitPanelProperties()
    {
        this_CanvasGroup.alpha = 0;
        this_Rect.anchoredPosition = Vector2.zero;
        
        InitSlidePanel();
    }

    public void OnClick_Open()
    {
        gameObject.SetActive(true);
        AnimateOpen();
    }

    public void OnClick_Close()
    {
        AnimateClose();
    }

    public void OnClick_ForceClose()
    {
        isAnimating = false;
        AnimateClose();
    }

    public void OnClick_ForceOpen()
    {
        isAnimating = false;
        OnClick_Open();
    }

    public void AnimateOpen()
    {
        switch(animationType)
        {
            case AnimationType.slide:
                AnimateOpen_Slide();
            break;

            case AnimationType.scale:
                AnimateOpen_Scale();
            break;

            case AnimationType.fade:
                AnimateOpen_Fade();
            break;
        }
    }

    public void AnimateClose()
    {
        switch(animationType)
        {
            case AnimationType.slide:
                AnimateClose_Slide();
            break;

            case AnimationType.scale:
                AnimateClose_Scale();
            break;

            case AnimationType.fade:
                AnimateClose_Fade();
            break;
        }
    }

    void AnimateOpen_Scale()
    {        
        if(isAnimating)
            return;

        isAnimating = true;

        PlaySFX(true);

        popupContent_Rect.localScale = initScale;

        popupContent_Rect.DOScale(finalScale * 1.1f, duration / 2).SetEase(easeIn_Type).OnComplete(() =>
        {
            popupContent_Rect.DOScale(finalScale, duration / 2).SetEase(easeOut_Type);
        });

        this_CanvasGroup.DOFade(1, duration).OnComplete(() =>
        {
            isAnimating = false;
        });
    }

    void AnimateClose_Scale()
    {
        if(isAnimating)
            return;

        isAnimating = true;

        PlaySFX(false);

        popupContent_Rect.DOScale(finalScale * 1.1f, duration / 2).SetEase(easeIn_Type).OnComplete(() =>
        {
            popupContent_Rect.DOScale(initScale, duration / 2).SetEase(easeOut_Type);
        });

        this_CanvasGroup.DOFade(0, duration).OnComplete(() =>
        {
            isAnimating = false;
            this.gameObject.SetActive(false);
        });
    }

    void AnimateOpen_Slide()
    {        
        if(isAnimating)
            return;

        isAnimating = true;

        PlaySFX(true);
        
        Vector2 startPosTemp = startPos;
        Vector2 endPosTemp = endPos;

        popupContent_Rect.anchoredPosition = startPosTemp;

        popupContent_Rect.DOAnchorPos(endPosTemp, duration).SetEase(easeIn_Type).OnComplete(() =>
        {
            isAnimating = false;
        });

        this_CanvasGroup.DOFade(1, duration);
    }

    void AnimateClose_Slide()
    {        
        if(isAnimating)
            return;

        isAnimating = true;

        PlaySFX(false);

        Vector2 endPosTemp = startPos;

        popupContent_Rect.DOAnchorPos(endPosTemp, duration).SetEase(easeOut_Type).OnComplete(() =>
        {
            isAnimating = false;
            this.gameObject.SetActive(false);
        });

        this_CanvasGroup.DOFade(0, duration);
    }

    

    void InitSlidePanel()
    {
        if(slideFrom == SlideFrom.manual)
            return;
        
        if(anchorPreset == AnchorPreset.partial)
        {
            InitSlidePanel_Partial();
            return;
        }

        if(anchorPreset == AnchorPreset.fullScreen)
        {
            InitSlidePanel_Full();
            return;
        }

        if(anchorPreset == AnchorPreset.popup)
        {
            InitSlidePanel_Popup();
            return;
        }
    }

    void InitSlidePanel_Partial()
    {
        switch(slideFrom)
        {
            case SlideFrom.left:
                startPos    = new Vector2(-popupContent_Rect.sizeDelta.x/2, 0);
                endPos      = new Vector2( popupContent_Rect.sizeDelta.x/2, 0);
            break;

            case SlideFrom.right:
                startPos    = new Vector2( popupContent_Rect.sizeDelta.x/2, 0);
                endPos      = new Vector2(-popupContent_Rect.sizeDelta.x/2, 0);
            break;

            case SlideFrom.top:
                startPos    = new Vector2(0,  popupContent_Rect.sizeDelta.y/2);
                endPos      = new Vector2(0, -popupContent_Rect.sizeDelta.y/2);
            break;

            case SlideFrom.bottom:
                startPos    = new Vector2(0, -popupContent_Rect.sizeDelta.y/2);
                endPos      = new Vector2(0,  popupContent_Rect.sizeDelta.y/2);
            break;
        }
    }

    void InitSlidePanel_Full()
    {
        switch(slideFrom)
        {
            case SlideFrom.left:
                startPos    = new Vector2(-popupContent_Rect.rect.size.x, 0);
                endPos      = Vector2.zero;
            break;

            case SlideFrom.right:
                startPos    = new Vector2(popupContent_Rect.rect.size.x, 0);
                endPos      = Vector2.zero;
            break;

            case SlideFrom.top:
                startPos    = new Vector2(0,  popupContent_Rect.rect.size.y);
                endPos      = Vector2.zero;
            break;

            case SlideFrom.bottom:
                startPos    = new Vector2(0, -popupContent_Rect.rect.size.y);
                endPos      = Vector2.zero;
            break;
        }
    }

    public void InitSlidePanel_Popup()
    {
        switch(slideFrom)
        {
            case SlideFrom.left:
                startPos    = new Vector2(-popupContent_Rect.sizeDelta.x - Screen.width, 0);
                endPos      = Vector2.zero;
            break;

            case SlideFrom.right:
                startPos    = new Vector2(popupContent_Rect.sizeDelta.x + Screen.width, 0);
                endPos      = Vector2.zero;
            break;

            case SlideFrom.top:
                startPos    = new Vector2(0,  popupContent_Rect.sizeDelta.y + Screen.height);
                endPos      = Vector2.zero;
            break;

            case SlideFrom.bottom:
                startPos    = new Vector2(0, -popupContent_Rect.sizeDelta.y - Screen.height);
                endPos      = Vector2.zero;
            break;
        }
    }

    void AnimateOpen_Fade()
    {        
        if(isAnimating)
            return;

        isAnimating = true;

        PlaySFX(true);

        this_CanvasGroup.DOFade(1, duration).OnComplete(()=>
        {
            isAnimating = false;
        });;
    }

    void AnimateClose_Fade()
    {        
        if(isAnimating)
            return;

        isAnimating = true;

        this_CanvasGroup.DOFade(0, duration).OnComplete(()=>
        {
            isAnimating = false;
            gameObject.SetActive(false);
        });
    }

    void PlaySFX(bool isOpenSFX)
    {
        AudioManager.Instance?.PlaySFX(isOpenSFX ? "open" : "close");
    }

    public enum AnimationType
    {
        scale,
        slide,
        fade
    }

    public enum SlideFrom
    {
        manual,
        left,
        right,
        top,
        bottom,
    }

    public enum AnchorPreset
    {
        partial,
        fullScreen,
        popup,
    }

// #if (UNITY_EDITOR)   
//     [CustomEditor(typeof(UIPanelAnimator))]
//     public class CustomInspector : Editor
//     {
//         public override void OnInspectorGUI()
//         {
//             DrawDefaultInspector();
    
//             UIPanelAnimator uiPanelAnimator = (UIPanelAnimator) target;
    
//             if(GUILayout.Button("Open"))
//             {
//                 uiPanelAnimator.AnimateOpen();
//             }

//             if(GUILayout.Button("Close"))
//             {
//                 uiPanelAnimator.AnimateClose();
//             }
//         }
//     }
// #endif
}
