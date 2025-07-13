using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroController : MonoBehaviour
{
    public TMP_Text displayText;
    public string[] messages; 
    public TMP_Text continueText; 

    public float fadeDuration = 1f;       
    public float messageDuration = 2f;    

    public CanvasGroup canvasGroup;
    private bool canContinue = false;
    [SerializeField] ChipStationUI chipStationUI;
    [SerializeField] GameObject introPanel,chipsPanel;
    //[SerializeField] List<Chip> initialChips;
    [SerializeField]ChipManager chipManager;

    [SerializeField] GameObject movementControllerPanel;


    void Awake()
    {
        //canvasGroup = displayText.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = displayText.gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        continueText.gameObject.SetActive(false);
        canvasGroup.alpha = 0;
        StartCoroutine(ShowMessagesWithFade());
    }

    IEnumerator ShowMessagesWithFade()
    {
        foreach (string message in messages)
        {
            displayText.text = message;

            yield return StartCoroutine(FadeCanvasGroup(displayText.GetComponent<CanvasGroup>(), 0, 1, fadeDuration));
            yield return new WaitForSeconds(messageDuration);

            yield return StartCoroutine(FadeCanvasGroup(displayText.GetComponent<CanvasGroup>(), 1, 0, fadeDuration));
        }

        displayText.text = "";
        continueText.gameObject.SetActive(true);
        canContinue = true;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cg.alpha = end;
    }

    void Update()
    {
        if (canContinue && Input.anyKeyDown)
        {
            canContinue = false;
            StartCoroutine(RevealChipsSequence());
        }
    }
    IEnumerator RevealChipsSequence()
    {
        //introPanel.SetActive(false);
        displayText.gameObject.SetActive(false);
        continueText.gameObject.SetActive(false);
        chipsPanel.SetActive(true);

        chipsPanel.GetComponent<CanvasGroup>().alpha = 0f;
        //canvasGroup.alpha = 0;

        List<Chip> chipsToEquip = new List<Chip>(chipStationUI.availableChips);

        chipStationUI.PopulateChips();

        // 1. Initial fade in (empty message, panel visible)
        displayText.text = "";
        yield return StartCoroutine(FadeCanvasGroup(chipsPanel.GetComponent<CanvasGroup>(), 0, 1, fadeDuration));

        // 2. Install each chip during fade-ins (no fade-out)
        foreach (var chip in chipsToEquip)
        {
            displayText.text = $"Installing: {chip.partName}";

            // Start fading in again (non-blocking)
            StartCoroutine(FadeCanvasGroup(chipsPanel.GetComponent<CanvasGroup>(), 0, 1, fadeDuration));

            // Equip chip while fading in
            chipManager.EquipChip(chip);
            chipStationUI.availableChips.Remove(chip);
            chipStationUI.PopulateChips();
            introPanel.SetActive(false);

            if (chip.partName.Contains("Logic"))
            {
                movementControllerPanel.SetActive(true);
            }

            yield return new WaitForSeconds(fadeDuration + 2f);
        }

        displayText.text = "";
    }



}
