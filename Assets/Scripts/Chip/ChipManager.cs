using UnityEngine;
using System.Collections.Generic;

public class ChipManager : MonoBehaviour
{
    public List<ChipData> equippedChips;

    public void EquipChip(ChipData chip)
    {
        if (!equippedChips.Contains(chip))
            equippedChips.Add(chip);
    }

    public void RemoveChip(ChipData chip)
    {
        if (chip.isCoreChip) return;
        equippedChips.Remove(chip);
    }
}
