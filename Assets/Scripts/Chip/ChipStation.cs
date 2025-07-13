using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipStation : MonoBehaviour
{
    [Header("Chip")]
    [SerializeField] List<ChipType> availableChipTypes;
    [SerializeField] List<Chip> avaiableChips;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        avaiableChips = ChipManager.Instance.Get_ChipSOList(availableChipTypes);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Player entered ChipStation area");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Player exited ChipStation area");
        }
    }


    #region Getters
    #endregion --- Getters ---
}