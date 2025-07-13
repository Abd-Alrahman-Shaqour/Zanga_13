using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] Animator animator;

    void OEnable()
    {
      
    }
    
    public void BreakShield()
    {
       this.gameObject.SetActive(false);
    }

    public void SetInactive()
    {
        this.gameObject.SetActive(false);
    }
}