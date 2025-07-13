using DG.Tweening;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] ThirdPersonController_RobotSphere thirdPersonController_RobotSphere;

    void Start()
    {
        
    }


    [Header("Equipped Chips")]
    public List<Chip> equippedChips;

    [Header("Controllers UI")]
    [SerializeField] GameObject controllersPanel, damageControllersPanel;


    public void EquipChip(Chip chip)
    {
        equippedChips.Add(chip);
        Debug.LogError(chip.partName);
    }

    public void RemoveChip(Chip chip)
    {
        equippedChips.Remove(chip);
    }

    public void UpdateChips()
    {
        bool hasOS = equippedChips.Any(chip => chip.partName == "OS");
        bool hasLogic = equippedChips.Any(chip => chip.partName == "Logic");
        bool hasVision = equippedChips.Any(chip => chip.partName == "Vision");
        bool hasJump = equippedChips.Any(chip => chip.partName == "Jump" || chip.partName == "Vertical Agility");
        bool hasShield = equippedChips.Any(chip => chip.partName == "Shield");

        if (!hasOS)
        {
            Debug.LogWarning("Missing OS chip � player dies.");
            // TODO: Handle death
            return;
        }

        if (!hasLogic)
        {
            Debug.LogWarning("Missing Logic chip � scramble controls.");
            damageControllersPanel.SetActive(true);

            var cg = damageControllersPanel.GetComponent<CanvasGroup>();
            cg.DOKill(); // Stop any existing tweens
            cg.alpha = 1;

            cg.DOFade(0.3f, 0.2f)
              .SetLoops(-1, LoopType.Yoyo)
              .SetEase(Ease.InOutSine);
        }
        else
        {
            controllersPanel.SetActive(true);

            // Optional: stop flicker and hide if Logic chip is back
            damageControllersPanel.GetComponent<CanvasGroup>()?.DOKill();
            damageControllersPanel.SetActive(false);
        }

        if (!hasVision)
        {
            VisualChipController.Instance.OnVisualChipRemoved();
        }
        else
        {
            VisualChipController.Instance.OnVisualChipReceived();
        }

        if (!hasJump)
        {
            Debug.LogWarning("Missing Jump chip � disable jumping.");
            // TODO: Disable jump input
        }

        if (!hasShield)
        {
            Debug.Log("No shield � player vulnerable.");
            // Optional effect
        }
        thirdPersonController_RobotSphere.AssignChipsValues(equippedChips);
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
    Dash
}

public enum RarityType
{
    Uncommon,
    Rare,
    Epic,
    Mythic,
}