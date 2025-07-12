using UnityEngine;


[CreateAssetMenu(fileName = "NewChip", menuName = "EXO/Chip")]
public class ChipData : ScriptableObject
{
    public string chipName;
    [TextArea] public string description;

    public ChipType type;
    public Sprite icon;
}
