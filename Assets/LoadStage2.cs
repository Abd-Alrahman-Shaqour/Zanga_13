using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadStage2 : MonoBehaviour
{
    void OTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameManager.Instance.TransitionManager.LoadScene("Stage2");
        }
    }
}
