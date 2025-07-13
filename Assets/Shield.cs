using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] Animator animator;

    void OEnable()
    {
        animator.Play("DoNothing");

       transform.localScale = Vector3.one * (0.15f + ((transform.GetSiblingIndex() + 1) * 0.05f));
    }
    
    public void BreakShield()
    {
        animator.Play("BreakShield");
    }

    public void SetInactive()
    {
        this.gameObject.SetActive(false);
    }
}