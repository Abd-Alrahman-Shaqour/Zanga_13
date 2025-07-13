using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChipStation : MonoBehaviour
{
    [Header("Chip")]
    [SerializeField] List<ChipType> availableChipTypes;
    [SerializeField] List<Chip> avaiableChips;

    [Header("Station")]
    [SerializeField] ChipStationUI chipStationUI;
    [SerializeField] GameObject station,interactE;
    [SerializeField] Button exploreLevelButton;
    [SerializeField] LinearCameraDolly linearCameraDolly;

    bool canOpenStation;


    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        avaiableChips = ChipManager.Instance.Get_ChipSOList(availableChipTypes);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canOpenStation)
        {
            Debug.LogError("Station Opened");
            station.SetActive(true);
            chipStationUI.PopulateChips();
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            exploreLevelButton.onClick.AddListener(linearCameraDolly.StartDolly);

            canOpenStation = true;
            interactE.SetActive(true);

            Debug.LogError("Player entered ChipStation area");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            exploreLevelButton.onClick.RemoveListener(linearCameraDolly.StartDolly);

            canOpenStation = false;
            interactE.SetActive(false);
            station.SetActive(false);


            Debug.LogError("Player exited ChipStation area");
        }
    }


    #region Getters
    #endregion --- Getters ---
}