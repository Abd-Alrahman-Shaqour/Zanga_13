using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_Audio : MonoBehaviour
{
    public void OnClick_Open()
    {
        UIManager.Instance.Open_PopupsAndPanels(UIType.Audio, true);
    }

    public void OnClick_Close()
    {
        UIManager.Instance.Close_PopupsAndPanels(UIType.Audio, true);
    }
}
