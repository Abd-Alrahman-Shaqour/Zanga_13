using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    [Header("Main components")]
    [SerializeField] ThirdPersonController_RobotSphere robotController;

    [Header("For testing")]
    [SerializeField] List<Chip> testing_ChipList;

    // Start is called before the first frame update
    void Start()
    {
        AssignChips(testing_ChipList);
    }

    public void AssignChips(List<Chip> _chipList)
    {
        robotController.AssignChipsValues(_chipList);
    }
}