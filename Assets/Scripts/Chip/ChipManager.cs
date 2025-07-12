using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipManager : MonoBehaviour
{
    #region Instance
    private static ChipManager _instance;
    public static ChipManager Instance
    {
        get
        {
            if (!_instance)
                _instance = GameObject.FindObjectOfType<ChipManager>();
            return _instance;
        }
    }
    #endregion Instance

    [Header("Main properties")]
    [SerializeField] List<Chip> allChipsList;

    [Header("Chip properties")]
    [SerializeField] List<Sprite> chip_BGList;

    void Start()
    {
        
    }


    public List<Chip> equippedChips;

    public void EquipChip(Chip chip)
    {
        if (!equippedChips.Contains(chip))
            equippedChips.Add(chip);
    }

    public void RemoveChip(Chip chip)
    {
        equippedChips.Remove(chip);
    }



    #region Getters
    public List<Chip> Get_ChipSOList(List<ChipType> _chipTypeList)
    {
        List<Chip> requiredChipList = new List<Chip>();

        foreach(ChipType chipType in _chipTypeList)
        {
            requiredChipList.Add(Get_ChipSO(chipType));
        }

        return requiredChipList;
    }

    public Chip Get_ChipSO(ChipType _chipType)
    {
        return allChipsList.Find(c => c.chipType == _chipType);
    }

    public Sprite Get_ChipRarityBG(RarityType _rarityType)
    {
        return chip_BGList[(int)_rarityType];
    }
#endregion --- Getters ---
}

public enum ChipType
{
    None,
    OS,
    Vision,
    Jump,
    Logic,
    Shield,
    Overclock,
}

public enum RarityType
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Mythic,
}