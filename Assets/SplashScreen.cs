using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DG.Tweening;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup.DOFade(1, fadeDuration).SetEase(Ease.InOutSine).SetUpdate(true);
    }
}
