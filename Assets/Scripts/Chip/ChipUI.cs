using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ChipUI : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
    public Image BGTint;
    public Image icon;
    public Image icon_shadow;
    public TextMeshProUGUI chipName;
    public TextMeshProUGUI chipDescription;

    [SerializeField] GameObject selectedFrame;

    public Chip chipData;
    private ChipStationUI station;

    private Tween shakeTween;

    [SerializeField] List<Color> colors;


    public void Setup(Chip data, ChipStationUI station)
    {
        this.chipData = data;
        this.station = station;

        icon.sprite = icon_shadow.sprite = data.icon;
        BGTint.color = colors[(int)chipData.rarityType];
        //chipName.text = data.chipName;
        //chipDescription.text = data.description;
    }

    public void OnClick()
    {
        station.OnChipUIClicked(this, chipData);
    }

    public void SetPressed(bool pressed)
    {
        if (pressed)
        {
            selectedFrame.gameObject.SetActive(true);

            if (shakeTween == null || !shakeTween.IsPlaying())
            {
                shakeTween = transform.DOShakeRotation(
                    duration: 20f,
                    strength: new Vector3(0, 0, 1f), 
                    vibrato: 10,
                    randomness: 90,
                    fadeOut: false
                ).SetLoops(-1); 
            }
        }
        else
        {
            selectedFrame.gameObject.SetActive(false);

            shakeTween?.Kill();
            transform.rotation = Quaternion.identity;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ChipHoverController.Show(chipData.description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ChipHoverController.Hide();
    }
}
