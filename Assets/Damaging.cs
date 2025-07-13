using System.Collections;
using UnityEngine;

public class Damaging : MonoBehaviour
{
    private Collider col;
    private Vector3 originalScale;
    private bool isRecharging = false;
    
    void Start()
    {
        col = GetComponent<Collider>();
        
        // Store original scale
        originalScale = transform.localScale;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !isRecharging)
        {
            other.GetComponent<Damageable>()?.TakeDamage();
            StartCoroutine(ScaleDownAndRecharge());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Player exited ChipStation area");
        }
    }
    
    IEnumerator ScaleDownAndRecharge()
    {
        isRecharging = true;
        
        // Disable collider
        col.enabled = false;
        
        // Scale down over 0.5 seconds
        float scaleTime = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < scaleTime)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0f, elapsedTime / scaleTime);
            transform.localScale = originalScale * scale;
            yield return null;
        }
        
        // Wait for remaining time (2.5 seconds)
        yield return new WaitForSeconds(2.5f);
        
        // Scale back up over 0.5 seconds
        elapsedTime = 0f;
        while (elapsedTime < scaleTime)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1f, elapsedTime / scaleTime);
            transform.localScale = originalScale * scale;
            yield return null;
        }
        
        // Re-enable collider
        col.enabled = true;
        isRecharging = false;
    }
}