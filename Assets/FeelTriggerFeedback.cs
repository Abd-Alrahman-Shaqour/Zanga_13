using UnityEngine;
using MoreMountains.Feedbacks;
using DG.Tweening;

public class FeelTriggerFeedback : MonoBehaviour
{
    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private LayerMask playerLayer = -1;

    [Header("Feel Feedbacks")]
    [SerializeField] private MMF_Player onEnterFeedback;
    [SerializeField] private MMF_Player onExitFeedback;
    [SerializeField] private MMF_Player onStayFeedback;

    [Header("Settings")]
    [SerializeField] private bool playOnEnter = true;
    [SerializeField] private bool playOnExit = false;
    [SerializeField] private bool playOnStay = false;
    [SerializeField] private float stayFeedbackInterval = 1f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private bool playerInside = false;
    private float stayTimer = 0f;

    void Start()
    {
         originalPosition = transform.position;
        // Ensure the collider is set as trigger
    }

    void Update()
    {
        // Handle stay feedback with interval
        if (playerInside && playOnStay && onStayFeedback != null)
        {
            stayTimer += Time.deltaTime;
            if (stayTimer >= stayFeedbackInterval)
            {
                onStayFeedback.PlayFeedbacks();
                stayTimer = 0f;

                if (showDebugLogs)
                    Debug.Log($"{gameObject.name}: Playing stay feedback");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
          Debug.Log($"{gameObject.name}: Player entered trigger - Playing enter feedback");
        if (IsPlayer(other))
        {
            playerInside = true;
            stayTimer = 0f;

            if (playOnEnter && onEnterFeedback != null)
            {
                onEnterFeedback.PlayFeedbacks();

                if (showDebugLogs)
                    Debug.Log($"{gameObject.name}: Player entered trigger - Playing enter feedback");
            }
            PushDown();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            playerInside = false;
            stayTimer = 0f;

            if (playOnExit && onExitFeedback != null)
            {
                onExitFeedback.PlayFeedbacks();

                if (showDebugLogs)
                    Debug.Log($"{gameObject.name}: Player exited trigger - Playing exit feedback");
            }
        }
    }

    private bool IsPlayer(Collider other)
    {
        // Check by tag
        if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag))
            return true;

        // Check by layer
        if (playerLayer != -1 && ((1 << other.gameObject.layer) & playerLayer) != 0)
            return true;

        return false;
    }

    // Public methods for external control
    public void PlayEnterFeedback()
    {
        if (onEnterFeedback != null)
        {
            onEnterFeedback.PlayFeedbacks();
        }
    }

    public void PlayExitFeedback()
    {
        if (onExitFeedback != null)
        {
            onExitFeedback.PlayFeedbacks();
        }
    }

    public void PlayStayFeedback()
    {
        if (onStayFeedback != null)
        {
            onStayFeedback.PlayFeedbacks();
        }
    }

    public void StopAllFeedbacks()
    {
        onEnterFeedback?.StopFeedbacks();
        onExitFeedback?.StopFeedbacks();
        onStayFeedback?.StopFeedbacks();
    }

    // Property to check if player is currently inside
    public bool PlayerInside => playerInside;
    [Header("Push Down Effect")]
    public float pushDownAmount = 0.2f;

    private Vector3 originalPosition;
    bool canpushdown = true;
    [SerializeField]bool pushdown = false;
    public void PushDown()
    {
        if (canpushdown && pushdown)
        {
            canpushdown = false;
            Vector3 targetPos = originalPosition + Vector3.down * pushDownAmount;
            transform.DOMove(targetPos, 0.2f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // Return to original position after a brief moment
                transform.DOMove(originalPosition, 0.9f).SetEase(Ease.OutBounce).OnComplete(() =>
                { canpushdown = true; });
            });
        }

    }
}