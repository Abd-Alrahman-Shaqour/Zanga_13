using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ChipUI : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TextMeshProUGUI chipName;
    public TextMeshProUGUI chipDescription;

    public Chip chipData;
    private ChipStationUI station;

    private Tween shakeTween;


    public void Setup(Chip data, ChipStationUI station)
    {
        this.chipData = data;
        this.station = station;

        icon.sprite = data.icon;
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
