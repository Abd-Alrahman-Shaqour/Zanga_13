// Individual checkpoint trigger script
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Feedbacks;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    public bool oneTimeUse = false;
    public GameObject checkpointEffect;

    private bool hasBeenUsed = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!oneTimeUse || !hasBeenUsed))
        {
            ActivateCheckpoint();
        }
    }

    void ActivateCheckpoint()
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SetCheckpoint(transform.position, transform.rotation);

            // Play checkpoint effect
            if (checkpointEffect != null)
            {
                checkpointEffect.SetActive(true);
            }

            hasBeenUsed = true;
            Debug.Log("Checkpoint activated!");
        }
    }
}