using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
public class VisualChipController : MonoBehaviour
{
    public static VisualChipController Instance;
    
    public UniversalRenderPipelineAsset urpAsset;
    
    private float currentRenderScale = 1.0f;

    void Start()
    {
        Instance = this;

        if (urpAsset == null)
        {
            Debug.LogError("Set URP ASSET");
        }

        if (urpAsset != null)
        {
            currentRenderScale = urpAsset.renderScale;
        }
    
    }
    
    public void OnVisualChipReceived()
    {
        if (urpAsset == null) return;
        
      
            urpAsset.renderScale = 0.1f;
            
            DOTween.To(
                () => urpAsset.renderScale,
                x => urpAsset.renderScale = x,
                1.0f,
                1.0f
            );
       
    }
    
    public void OnVisualChipRemoved()
    {
        if (urpAsset == null) return;
        
        DOTween.To(
            () => urpAsset.renderScale,
            x => urpAsset.renderScale = x,
            0.1f,
            1.0f
        );
    }
}
