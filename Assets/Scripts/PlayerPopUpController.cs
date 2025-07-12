using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPopUpController : MonoBehaviour
{
    [SerializeField] GameObject interactE;
    [SerializeField] ChipStationUI chipStationUI;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E)) 
        {
            chipStationUI.PopulateChips();
            interactE.SetActive(true);
        }    
    }
}
