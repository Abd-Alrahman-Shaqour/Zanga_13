using UnityEngine;

public enum ChipType
{
    OS, Vision, Jump, DoubleJump, Logic, Shield, Speed, NightVision, BoosterDash
}

[CreateAssetMenu(fileName = "NewChip", menuName = "EXO/Chip")]
public class ChipData : ScriptableObject
{
    public string chipName;
    [TextArea] public string description;
    [TextArea] public string drawback;

    public ChipType type;
    public Sprite icon;

    public bool isCoreChip; // Prevents it from being removed (e.g., OS)
}
