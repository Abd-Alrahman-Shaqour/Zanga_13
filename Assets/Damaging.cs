using System.Collections;
using UnityEngine;

public class Damaging : MonoBehaviour
{
    private Collider col;
    private Renderer rend;
    private Material mat;
    private float originalAlpha;
    private bool isRecharging = false;
    
    void Start()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        mat = rend.material;
        
        // Store original alpha value
        originalAlpha = mat.color.a;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !isRecharging)
        {
            other.GetComponent<Damageable>().TakeDamage();
            StartCoroutine(FadeOutAndRecharge());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Player exited ChipStation area");
        }
    }
    
    IEnumerator FadeOutAndRecharge()
    {
        isRecharging = true;
        
        // Disable collider
        col.enabled = false;
        
        // Fade out over 0.5 seconds
        float fadeTime = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(originalAlpha, 0f, elapsedTime / fadeTime);
            Color newColor = mat.color;
            newColor.a = alpha;
            mat.color = newColor;
            yield return null;
        }
        
        // Wait for remaining time (2.5 seconds)
        yield return new WaitForSeconds(2.5f);
        
        // Fade back in over 0.5 seconds
        elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, originalAlpha, elapsedTime / fadeTime);
            Color newColor = mat.color;
            newColor.a = alpha;
            mat.color = newColor;
            yield return null;
        }
        
        // Re-enable collider
        col.enabled = true;
        isRecharging = false;
    }
}