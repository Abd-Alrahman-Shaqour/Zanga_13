using UnityEngine;

public class DollyTriggerTest : MonoBehaviour
{
    [Header("Dolly Reference")]
    [Tooltip("The LinearCameraDolly script to trigger")]
    public LinearCameraDolly cameraDolly;
    
    [Header("Trigger Settings")]
    [Tooltip("Only objects with this tag will trigger the dolly")]
    public string triggerTag = "Player";
    
    [Tooltip("Only trigger once, then disable")]
    public bool triggerOnce = true;
    
    [Tooltip("Show debug messages in console")]
    public bool showDebugMessages = true;
    
    [Header("Optional Delay")]
    [Tooltip("Delay in seconds before starting the dolly")]
    public float delayBeforeStart = 0f;
    
    private bool hasTriggered = false;
    
    void Start()
    {
        // Auto-find the dolly script if not assigned
        if (cameraDolly == null)
        {
            cameraDolly = FindObjectOfType<LinearCameraDolly>();
            if (cameraDolly == null)
            {
                Debug.LogError("DollyTriggerTest: No LinearCameraDolly found in scene! Please assign one manually.");
            }
        }
        
        // Ensure this GameObject has a collider set as trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning("DollyTriggerTest: No Collider component found. Adding BoxCollider and setting as trigger.");
            BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
            boxCol.isTrigger = true;
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning("DollyTriggerTest: Collider is not set as trigger. Setting isTrigger to true.");
            col.isTrigger = true;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if we should trigger
        if (!ShouldTrigger(other.gameObject))
            return;
        
        if (showDebugMessages)
        {
            Debug.Log($"DollyTriggerTest: Triggered by {other.gameObject.name}");
        }
        
        // Mark as triggered if set to trigger once
        if (triggerOnce)
        {
            hasTriggered = true;
        }
        
        // Start the dolly with optional delay
        if (delayBeforeStart > 0)
        {
            StartCoroutine(StartDollyWithDelay());
        }
        else
        {
            StartDolly();
        }
    }
    
    private bool ShouldTrigger(GameObject triggerObject)
    {
        // Check if already triggered and set to trigger once
        if (triggerOnce && hasTriggered)
        {
            if (showDebugMessages)
            {
                Debug.Log("DollyTriggerTest: Already triggered once, ignoring.");
            }
            return false;
        }
        
        // Check if cameraDolly is assigned
        if (cameraDolly == null)
        {
            Debug.LogError("DollyTriggerTest: No LinearCameraDolly assigned!");
            return false;
        }
        
        // Check tag if specified
        if (!string.IsNullOrEmpty(triggerTag))
        {
            if (!triggerObject.CompareTag(triggerTag))
            {
                if (showDebugMessages)
                {
                    Debug.Log($"DollyTriggerTest: Object {triggerObject.name} doesn't have required tag '{triggerTag}'");
                }
                return false;
            }
        }
        
        return true;
    }
    
    private System.Collections.IEnumerator StartDollyWithDelay()
    {
        if (showDebugMessages)
        {
            Debug.Log($"DollyTriggerTest: Starting dolly in {delayBeforeStart} seconds...");
        }
        
        yield return new WaitForSeconds(delayBeforeStart);
        StartDolly();
    }
    
    private void StartDolly()
    {
        if (cameraDolly != null)
        {
            if (showDebugMessages)
            {
                Debug.Log("DollyTriggerTest: Starting camera dolly!");
            }
            
            cameraDolly.StartDolly();
        }
    }
    
    // Public methods for external control
    public void ResetTrigger()
    {
        hasTriggered = false;
        if (showDebugMessages)
        {
            Debug.Log("DollyTriggerTest: Trigger reset - can trigger again.");
        }
    }
    
    public void ManualTrigger()
    {
        if (showDebugMessages)
        {
            Debug.Log("DollyTriggerTest: Manual trigger called.");
        }
        
        if (cameraDolly != null)
        {
            StartDolly();
        }
    }
    
    public void SetTriggerTag(string newTag)
    {
        triggerTag = newTag;
    }
    
    public void SetDollyReference(LinearCameraDolly newDolly)
    {
        cameraDolly = newDolly;
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            // Draw trigger area
            Gizmos.color = hasTriggered ? Color.red : Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
            else if (col is CapsuleCollider capsule)
            {
                Gizmos.DrawWireCube(capsule.center, new Vector3(capsule.radius * 2, capsule.height, capsule.radius * 2));
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw connection line to dolly camera if assigned
        if (cameraDolly != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, cameraDolly.transform.position);
        }
    }
}