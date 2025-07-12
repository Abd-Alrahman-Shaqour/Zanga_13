using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ChipUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI chipName;
    public TextMeshProUGUI chipDescription;

    public ChipData chipData;
    private ChipStationUI station;

    private Tween shakeTween;


    public void Setup(ChipData data, ChipStationUI station)
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

            // Prevent duplicate shakes
            if (shakeTween == null || !shakeTween.IsPlaying())
            {
                shakeTween = transform.DOShakeRotation(
                    duration: 1f,
                    strength: new Vector3(0, 0, 5f), // Shake Z-axis like wiggle
                    vibrato: 10,
                    randomness: 90,
                    fadeOut: false
                ).SetLoops(-1); // Loop forever
            }
        }
        else
        {
            shakeTween?.Kill();
            transform.rotation = Quaternion.identity;
        }
    }
}
