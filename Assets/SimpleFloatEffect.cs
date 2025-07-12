using UnityEngine;
using DG.Tweening;

public class SimpleFloatEffect : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatStrength = 1f;
    public float floatDuration = 2f;
    
    private Vector3 startPos;
    
    void Start()
    {
        startPos = transform.position;
        StartFloating();
    }
    
    void StartFloating()
    {
        // Create a looping float animation
        transform.DOMoveY(startPos.y + floatStrength, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    void OnDestroy()
    {
        // Kill the tween when object is destroyed
        transform.DOKill();
    }
}