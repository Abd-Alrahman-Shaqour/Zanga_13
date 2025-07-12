using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class ChipStationUI : MonoBehaviour
{
    public ChipManager playerChipManager;

    public List<Transform> playerChipSlots;
    public List<Transform> availableChipSlots;
    public ChipUI chipUIPrefab;

    private ChipUI selectedUI;


    private Chip selectedChip;
    private ChipSource selectedChipSource;

    public enum ChipSource { Player, Available }

    public List<Chip> availableChips = new();

/*    private void Start()
    {
        PopulateChips();
    }*/

    public void PopulateChips()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (Transform t in playerChipSlots) DestroyChildren(t);
        foreach (Transform t in availableChipSlots) DestroyChildren(t);

        for (int i = 0; i < playerChipManager.equippedChips.Count && i < playerChipSlots.Count; i++)
        {
            var chip = playerChipManager.equippedChips[i];
            var ui = Instantiate(chipUIPrefab, playerChipSlots[i]);
            ui.Setup(chip, this);
        }


        foreach (var chip in availableChips)
        {
            var ui = Instantiate(chipUIPrefab, availableChipSlots[0]);
            ui.Setup(chip, this);
        }
    }

    private void DestroyChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnChipUIClicked(ChipUI chipUI, Chip chip)
    {
        bool isPlayerSide = IsPlayerChip(chip);

        if (selectedChip == null)
        {
            // First selection
            selectedChip = chip;
            selectedChipSource = isPlayerSide ? ChipSource.Player : ChipSource.Available;
            selectedUI = chipUI;
            chipUI.SetPressed(true);
        }
        else
        {
            bool clickedSameSide = (selectedChipSource == (isPlayerSide ? ChipSource.Player : ChipSource.Available));

            if (!clickedSameSide)
            {
                // SWAP the chips visually & logically
                SwapWithEffect(selectedChip, chip, selectedUI, chipUI);
            }

            // Reset selection
            selectedChip = null;
            selectedUI?.SetPressed(false);
            selectedUI = null;
        }
    }

    void SwapWithEffect(Chip a, Chip b, ChipUI aUI, ChipUI bUI)
    {
        Vector3 aStartPos = aUI.transform.position;
        Vector3 bStartPos = bUI.transform.position;

        aUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
        bUI.GetComponent<CanvasGroup>().blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();

        seq.Append(aUI.transform.DOMove(bStartPos, 0.3f).SetEase(Ease.InOutSine));
        seq.Join(bUI.transform.DOMove(aStartPos, 0.3f).SetEase(Ease.InOutSine));

        seq.AppendCallback(() =>
        {
            // Swap chips between lists, keeping order intact
            if (playerChipManager.equippedChips.Contains(a) && availableChips.Contains(b))
            {
                int aIndex = playerChipManager.equippedChips.IndexOf(a);
                int bIndex = availableChips.IndexOf(b);

                // Swap chips in the lists by index
                playerChipManager.equippedChips[aIndex] = b;
                availableChips[bIndex] = a;
            }
            else if (playerChipManager.equippedChips.Contains(b) && availableChips.Contains(a))
            {
                int bIndex = playerChipManager.equippedChips.IndexOf(b);
                int aIndex = availableChips.IndexOf(a);

                playerChipManager.equippedChips[bIndex] = a;
                availableChips[aIndex] = b;
            }
        });

        seq.AppendInterval(0.05f);

        seq.Append(aUI.transform.DOLocalMove(Vector3.zero, 0.01f));
        seq.Join(bUI.transform.DOLocalMove(Vector3.zero, 0.01f));

        seq.AppendCallback(() =>
        {
            aUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
            bUI.GetComponent<CanvasGroup>().blocksRaycasts = true;

            PopulateChips();
        });
    }


    bool IsPlayerChip(Chip chip) => playerChipManager.equippedChips.Contains(chip);
}
