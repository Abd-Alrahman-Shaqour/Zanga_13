using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_Demo : MonoBehaviour
{
    public void OnClick_Open()
    {
        UIManager.Instance.Open_PopupsAndPanels(UIType.Settings, true);
    }

    public void OnClick_Close()
    {
        UIManager.Instance.Close_PopupsAndPanels(UIType.Settings, true);
    }
}
