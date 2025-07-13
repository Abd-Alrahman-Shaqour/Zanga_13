using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using StarterAssets;

public class LinearCameraDolly : MonoBehaviour
{
    [Header("Cinemachine Settings")]
    public CinemachineVirtualCameraBase virtualCamera;
    public CinemachineVirtualCameraBase mainVirtualCamera;
    public int activePriority = 10;
    public int inactivePriority = 0;
    public bool manageOtherCameras = true;
    public bool returnToMainCamera = true;
    
    [Header("Dolly Settings")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public bool playOnStart = true;
    public bool loop = false;
    public bool pingPong = false;
    
    [Header("Look At Settings")]
    public Transform lookAtTarget;
    public bool smoothLookAt = true;
    public float lookAtSpeed = 2f;
    
    [Header("Controls")]
    public KeyCode startKey = KeyCode.Space;
    public KeyCode stopKey = KeyCode.Escape;
    public KeyCode resetKey = KeyCode.R;
    
    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private bool isReversed = false;
    private Vector3 startPosition;
    private Quaternion startRotation;

    [SerializeField] ThirdPersonController_RobotSphere thirdPersonController_RobotSphere;


    void Start()
    {
        // Store initial position and rotation
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        // Auto-find virtual camera if not assigned
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCameraBase>();
            if (virtualCamera == null)
            {
                Debug.LogWarning("No CinemachineVirtualCameraBase found on this GameObject. Please assign one manually.");
            }
        }
        
        // Validate waypoints
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogError("LinearCameraDolly requires at least 2 waypoints!");
            return;
        }
        
        // Set initial position to first waypoint
        transform.position = waypoints[0].position;
        
        // Start movement if enabled
        if (playOnStart)
        {
            StartDolly();
        }
    }
    
    void Update()
    {

        
        // Handle look at target
        if (lookAtTarget != null && !isMoving)
        {
            HandleLookAt();
        }
    }
    
    public void StartDolly()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        
        // Set this virtual camera as active
        SetVirtualCameraActive(true);
        
        if (!isMoving)
        {
            StartCoroutine(MoveThroughWaypoints());
            thirdPersonController_RobotSphere.canPlay = false;

        }
    }
    
    public void StopDolly()
    {
        StopAllCoroutines();
        isMoving = false;
        
        // Return to main camera when stopped
        if (returnToMainCamera)
        {
            ReturnToMainCamera();
            thirdPersonController_RobotSphere.canPlay = true;

        }
    }
    
    public void ResetDolly()
    {
        StopAllCoroutines();
        isMoving = false;
        currentWaypointIndex = 0;
        isReversed = false;
        
        // Reset to first waypoint or original position
        if (waypoints != null && waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
        }
        else
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
        }
    }
    
    private IEnumerator MoveThroughWaypoints()
    {
        isMoving = true;
        
        while (isMoving)
        {
            // Get target waypoint
            int targetIndex = GetNextWaypointIndex();
            
            if (targetIndex == -1)
            {
                // End of sequence - return to main camera
                isMoving = false;
                OnDollyComplete();
                yield break;
            }
            
            Vector3 targetPosition = waypoints[targetIndex].position;
            
            // Move to target waypoint in straight line
            yield return StartCoroutine(MoveToPosition(targetPosition));
            
            // Update current waypoint index
            currentWaypointIndex = targetIndex;
            
            // Check if we've reached the end
            if (!loop && !pingPong)
            {
                if ((!isReversed && currentWaypointIndex >= waypoints.Length - 1) ||
                    (isReversed && currentWaypointIndex <= 0))
                {
                    isMoving = false;
                    OnDollyComplete();
                    yield break;
                }
            }
        }
    }
    
    private int GetNextWaypointIndex()
    {
        if (pingPong)
        {
            if (!isReversed)
            {
                if (currentWaypointIndex < waypoints.Length - 1)
                {
                    return currentWaypointIndex + 1;
                }
                else
                {
                    isReversed = true;
                    return currentWaypointIndex - 1;
                }
            }
            else
            {
                if (currentWaypointIndex > 0)
                {
                    return currentWaypointIndex - 1;
                }
                else
                {
                    isReversed = false;
                    return currentWaypointIndex + 1;
                }
            }
        }
        else if (loop)
        {
            return (currentWaypointIndex + 1) % waypoints.Length;
        }
        else
        {
            // Regular sequence
            if (currentWaypointIndex < waypoints.Length - 1)
            {
                return currentWaypointIndex + 1;
            }
            else
            {
                return -1; // End of sequence
            }
        }
    }
    
    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, targetPosition);
        float duration = distance / moveSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // Linear interpolation (straight line movement)
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            
            // Handle look at target during movement
            if (lookAtTarget != null)
            {
                HandleLookAt();
            }
            
            yield return null;
        }
        
        // Ensure we reach the exact target position
        transform.position = targetPosition;
    }
    
    private void HandleLookAt()
    {
        if (lookAtTarget == null) return;
        
        Vector3 direction = (lookAtTarget.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        
        if (smoothLookAt)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookAtSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }
    
    // Cinemachine camera management
    private void OnDollyComplete()
    {
        if (returnToMainCamera)
        {
            ReturnToMainCamera();
        }
    }
    
    private void ReturnToMainCamera()
    {
        if (mainVirtualCamera == null)
        {
            Debug.LogWarning("Main Virtual Camera not assigned. Cannot return to main camera.");
            return;
        }
        
        // Set main camera as active
        mainVirtualCamera.Priority = activePriority;
        mainVirtualCamera.MoveToTopOfPrioritySubqueue();
        
        // Set this dolly camera to inactive
        if (virtualCamera != null)
        {
            virtualCamera.Priority = inactivePriority;
        }
        
        // Set all other cameras to inactive if managing them
        if (manageOtherCameras)
        {
            SetOtherCameraPriorities(inactivePriority, mainVirtualCamera);
        }

        thirdPersonController_RobotSphere.canPlay = true;
    }
    
    private void SetVirtualCameraActive(bool active)
    {
        if (virtualCamera == null) return;
        
        if (active)
        {
            // Set this camera's priority high
            virtualCamera.Priority = activePriority;
            virtualCamera.MoveToTopOfPrioritySubqueue();
            
            // Set all other virtual cameras to low priority
            if (manageOtherCameras)
            {
                SetOtherCameraPriorities(inactivePriority, virtualCamera);
            }
        }
        else
        {
            // Set this camera's priority low
            virtualCamera.Priority = inactivePriority;
        }
    }
    
    private void SetOtherCameraPriorities(int priority, CinemachineVirtualCameraBase excludeCamera = null)
    {
        // Find all virtual cameras in the scene
        CinemachineVirtualCameraBase[] allVCams = FindObjectsOfType<CinemachineVirtualCameraBase>();
        
        foreach (var vcam in allVCams)
        {
            // Skip the excluded camera (either our dolly camera or main camera)
            if (vcam == excludeCamera) continue;
            
            // Set priority
            vcam.Priority = priority;
        }
    }
    
    // Public methods for external control
    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        ResetDolly();
    }
    
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0.1f, speed);
    }
    
    public void SetLoop(bool enableLoop)
    {
        loop = enableLoop;
        if (loop) pingPong = false;
    }
    
    public void SetPingPong(bool enablePingPong)
    {
        pingPong = enablePingPong;
        if (pingPong) loop = false;
    }
    
    public void SetVirtualCamera(CinemachineVirtualCameraBase vcam)
    {
        virtualCamera = vcam;
    }
    
    public void SetMainVirtualCamera(CinemachineVirtualCameraBase mainVCam)
    {
        mainVirtualCamera = mainVCam;
    }
    
    public void ActivateVirtualCamera()
    {
        SetVirtualCameraActive(true);
    }
    
    public void DeactivateVirtualCamera()
    {
        SetVirtualCameraActive(false);
    }
    
    public void ForceReturnToMainCamera()
    {
        ReturnToMainCamera();
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        
        Gizmos.color = Color.yellow;
        
        // Draw waypoints
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);
                
                // Draw waypoint number
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(waypoints[i].position + Vector3.up * 0.5f, i.ToString());
                #endif
            }
        }
        
        // Draw lines between waypoints
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
        
        // Draw loop connection if enabled
        if (loop && waypoints.Length > 2 && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
    }
}