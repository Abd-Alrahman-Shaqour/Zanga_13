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

#region Getters
#endregion --- Getters ---
}