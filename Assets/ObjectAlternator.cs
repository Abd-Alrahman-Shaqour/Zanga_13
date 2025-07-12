using UnityEngine;
using System.Collections;

public class LaserHazardAlternator : MonoBehaviour
{
    [Header("Laser Beam Objects")]
    public GameObject laserA;
    public GameObject laserB;
    
    [Header("Timing Settings")]
    [Range(0.5f, 10f)]
    public float activeTime = 2f; // Time each laser stays fully active
    [Range(0.1f, 2f)]
    public float transitionTime = 0.3f; // Time for fade in/out transition
    
    [Header("Visual Effects")]
    [Range(0f, 1f)]
    public float minAlpha = 0.05f; // Minimum alpha when inactive
    public bool useScaleEffect = true; // Scale down when fading out
    [Range(0.1f, 1f)]
    public float minScale = 0.2f; // Minimum scale when fading out
    public bool useColorShift = false; // Optional color shift when fading
    public Color fadeColor = Color.red; // Color when fading out
    
    [Header("Debug Info")]
    [SerializeField] private bool isLaserAActive = true;
    [SerializeField] private float timer = 0f;
    [SerializeField] private LaserState currentState = LaserState.LaserAActive;
    
    private Renderer[] renderersA;
    private Renderer[] renderersB;
    private Color[] originalColorsA;
    private Color[] originalColorsB;
    private Color[] originalEmissionA;
    private Color[] originalEmissionB;
    private Vector3[] originalScalesA;
    private Vector3[] originalScalesB;
    private bool[] hasEmissionA;
    private bool[] hasEmissionB;
    
    enum LaserState
    {
        LaserAActive,
        Transitioning,
        LaserBActive
    }
    
    void Start()
    {
        InitializeLasers();
        SetupInitialState();
    }
    
    void InitializeLasers()
    {
        if (laserA != null)
        {
            renderersA = laserA.GetComponentsInChildren<Renderer>();
            originalColorsA = new Color[renderersA.Length];
            originalEmissionA = new Color[renderersA.Length];
            originalScalesA = new Vector3[renderersA.Length];
            hasEmissionA = new bool[renderersA.Length];
            
            for (int i = 0; i < renderersA.Length; i++)
            {
                originalColorsA[i] = renderersA[i].material.color;
                originalScalesA[i] = renderersA[i].transform.localScale;
                
                // Check if material has emission
                if (renderersA[i].material.HasProperty("_EmissionColor"))
                {
                    originalEmissionA[i] = renderersA[i].material.GetColor("_EmissionColor");
                    hasEmissionA[i] = true;
                }
                else
                {
                    hasEmissionA[i] = false;
                }
            }
        }
        
        if (laserB != null)
        {
            renderersB = laserB.GetComponentsInChildren<Renderer>();
            originalColorsB = new Color[renderersB.Length];
            originalEmissionB = new Color[renderersB.Length];
            originalScalesB = new Vector3[renderersB.Length];
            hasEmissionB = new bool[renderersB.Length];
            
            for (int i = 0; i < renderersB.Length; i++)
            {
                originalColorsB[i] = renderersB[i].material.color;
                originalScalesB[i] = renderersB[i].transform.localScale;
                
                // Check if material has emission
                if (renderersB[i].material.HasProperty("_EmissionColor"))
                {
                    originalEmissionB[i] = renderersB[i].material.GetColor("_EmissionColor");
                    hasEmissionB[i] = true;
                }
                else
                {
                    hasEmissionB[i] = false;
                }
            }
        }
    }
    
    void SetupInitialState()
    {
        if (laserA != null && laserB != null)
        {
            // Both lasers are always active GameObjects
            laserA.SetActive(true);
            laserB.SetActive(true);
            
            // Laser A starts active, Laser B starts inactive
            SetLaserVisibility(laserA, renderersA, originalColorsA, originalEmissionA, originalScalesA, hasEmissionA, 1f, true);
            SetLaserVisibility(laserB, renderersB, originalColorsB, originalEmissionB, originalScalesB, hasEmissionB, minAlpha, false);
            
            currentState = LaserState.LaserAActive;
            isLaserAActive = true;
        }
        else
        {
            Debug.LogError("Please assign both laser objects in the inspector!");
        }
    }
    
    void Update()
    {
        if (laserA == null || laserB == null) return;
        
        timer += Time.deltaTime;
        
        switch (currentState)
        {
            case LaserState.LaserAActive:
                if (timer >= activeTime)
                {
                    StartCoroutine(TransitionLasers(true)); // A to B
                }
                break;
                
            case LaserState.LaserBActive:
                if (timer >= activeTime)
                {
                    StartCoroutine(TransitionLasers(false)); // B to A
                }
                break;
        }
    }
    
    IEnumerator TransitionLasers(bool fadeAtoB)
    {
        currentState = LaserState.Transitioning;
        
        float elapsed = 0f;
        
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionTime;
            
            if (fadeAtoB)
            {
                // Fade A out, Fade B in - SIMULTANEOUSLY
                float alphaA = Mathf.Lerp(1f, minAlpha, t);
                float alphaB = Mathf.Lerp(minAlpha, 1f, t);
                
                // A is becoming inactive, B is becoming active
                bool aIsActive = alphaA > 0.5f; // Active when above 50% visibility
                bool bIsActive = alphaB > 0.5f;
                
                SetLaserVisibility(laserA, renderersA, originalColorsA, originalEmissionA, originalScalesA, hasEmissionA, alphaA, aIsActive);
                SetLaserVisibility(laserB, renderersB, originalColorsB, originalEmissionB, originalScalesB, hasEmissionB, alphaB, bIsActive);
            }
            else
            {
                // Fade B out, Fade A in - SIMULTANEOUSLY
                float alphaB = Mathf.Lerp(1f, minAlpha, t);
                float alphaA = Mathf.Lerp(minAlpha, 1f, t);
                
                // B is becoming inactive, A is becoming active
                bool bIsActive = alphaB > 0.5f; // Active when above 50% visibility
                bool aIsActive = alphaA > 0.5f;
                
                SetLaserVisibility(laserA, renderersA, originalColorsA, originalEmissionA, originalScalesA, hasEmissionA, alphaA, aIsActive);
                SetLaserVisibility(laserB, renderersB, originalColorsB, originalEmissionB, originalScalesB, hasEmissionB, alphaB, bIsActive);
            }
            
            yield return null;
        }
        
        // Ensure final state is set properly
        if (fadeAtoB)
        {
            SetLaserVisibility(laserA, renderersA, originalColorsA, originalEmissionA, originalScalesA, hasEmissionA, minAlpha, false);
            SetLaserVisibility(laserB, renderersB, originalColorsB, originalEmissionB, originalScalesB, hasEmissionB, 1f, true);
            currentState = LaserState.LaserBActive;
            isLaserAActive = false;
        }
        else
        {
            SetLaserVisibility(laserA, renderersA, originalColorsA, originalEmissionA, originalScalesA, hasEmissionA, 1f, true);
            SetLaserVisibility(laserB, renderersB, originalColorsB, originalEmissionB, originalScalesB, hasEmissionB, minAlpha, false);
            currentState = LaserState.LaserAActive;
            isLaserAActive = true;
        }
        
        timer = 0f;
    }
    
    void SetLaserVisibility(GameObject laser, Renderer[] renderers, Color[] originalColors, Color[] originalEmissions, Vector3[] originalScales, bool[] hasEmission, float alpha, bool isActive)
    {
        if (renderers == null) return;
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                // Set alpha
                Color currentColor = originalColors[i];
                if (useColorShift && alpha < 1f)
                {
                    currentColor = Color.Lerp(fadeColor, originalColors[i], alpha);
                }
                currentColor.a = alpha;
                renderers[i].material.color = currentColor;
                
                // Set emission based on active state
                if (hasEmission[i])
                {
                    if (isActive)
                    {
                        // Active: full emission
                        renderers[i].material.SetColor("_EmissionColor", originalEmissions[i]);
                    }
                    else
                    {
                        // Inactive: no emission
                        renderers[i].material.SetColor("_EmissionColor", Color.black);
                    }
                }
                
                // Set scale
                if (useScaleEffect)
                {
                    float scaleMultiplier = Mathf.Lerp(minScale, 1f, alpha);
                    renderers[i].transform.localScale = originalScales[i] * scaleMultiplier;
                }
            }
        }
    }
    
    // Public methods to check which laser is currently active (for player collision detection)
    public bool IsLaserAActive()
    {
        return isLaserAActive;
    }
    
    public bool IsLaserBActive()
    {
        return !isLaserAActive;
    }
    
    // Get the currently active laser GameObject
    public GameObject GetActiveLaser()
    {
        return isLaserAActive ? laserA : laserB;
    }
    
    // Get the currently inactive laser GameObject
    public GameObject GetInactiveLaser()
    {
        return isLaserAActive ? laserB : laserA;
    }
    
    // Optional: Method to manually trigger a switch
    [ContextMenu("Manual Switch")]
    public void ManualSwitch()
    {
        if (currentState == LaserState.LaserAActive)
        {
            StartCoroutine(TransitionLasers(true));
        }
        else if (currentState == LaserState.LaserBActive)
        {
            StartCoroutine(TransitionLasers(false));
        }
    }
    
    // Optional: Method to change timing at runtime
    public void SetActiveTime(float newActiveTime)
    {
        activeTime = Mathf.Max(0.5f, newActiveTime);
    }
    
    public void SetTransitionTime(float newTransitionTime)
    {
        transitionTime = Mathf.Max(0.1f, newTransitionTime);
    }
}