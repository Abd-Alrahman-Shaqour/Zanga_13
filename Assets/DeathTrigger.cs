using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    [Header("Death Trigger Settings")]
    public bool destroyOnTrigger = false;
    public GameObject deathEffect;
    public AudioClip deathSound;
    
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerPlayerDeath(other.gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TriggerPlayerDeath(collision.gameObject);
        }
    }
    
    void TriggerPlayerDeath(GameObject player)
    {
        // Play death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, player.transform.position, Quaternion.identity);
        }
        
        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Call the death function
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnPlayerDeath();
        }
        else
        {
            Debug.LogError("CheckpointManager not found!");
        }
        
        // Destroy this object if specified
        if (destroyOnTrigger)
        {
            Destroy(gameObject, 0.1f);
        }
    }
}

