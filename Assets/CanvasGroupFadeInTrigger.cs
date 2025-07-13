using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CanvasGroupFadeInTrigger : MonoBehaviour
{
    [Header("Canvas Group Settings")]
    public CanvasGroup canvasGroup;
    
    [Header("Fade Settings")]
    public float fadeDuration = 1f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Trigger Settings")]
    public string playerTag = "Player";
    public bool fadeOnEnter = true;
    public bool fadeOnExit = false;
    public bool resetOnExit = false;
    
    private Coroutine fadeCoroutine;
    
    void Start()
    {
        // Ensure the canvas group starts invisible if fadeOnEnter is true
        if (fadeOnEnter && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        // Make sure this GameObject has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (fadeOnEnter)
            {
                FadeIn();
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (fadeOnExit)
            {
                FadeOut();
            }
            else if (resetOnExit)
            {
                ResetCanvas();
            }
        }
    }
    
    public void FadeIn()
    {
        if (canvasGroup == null) return;
        
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeCoroutine(canvasGroup.alpha, 1f));
    }
    
    public void FadeOut()
    {
        if (canvasGroup == null) return;
        
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeCoroutine(canvasGroup.alpha, 0f));
    }
    
    public void ResetCanvas()
    {
        if (canvasGroup == null) return;
        
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        canvasGroup.alpha = 0f;
    }
    
    private IEnumerator FadeCoroutine(float startAlpha, float targetAlpha)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;
            
            // Use animation curve for smooth easing
            float curveValue = fadeCurve.Evaluate(progress);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
            
            yield return null;
        }
        
        // Ensure we end at the exact target alpha
        canvasGroup.alpha = targetAlpha;
        fadeCoroutine = null;
    }
    
    // Optional: Preview in editor
    void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}