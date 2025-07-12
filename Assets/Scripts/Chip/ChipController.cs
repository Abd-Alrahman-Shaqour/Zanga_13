using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipController : MonoBehaviour
{
    [SerializeField] List<Chip> myChips = new List<Chip>();

    public void Set_RobotChips(List<Chip> _chipList)
    {
        myChips = _chipList;
    }

    public List<Chip> Get_RobotChips()
    {
        return myChips;
    }
}
