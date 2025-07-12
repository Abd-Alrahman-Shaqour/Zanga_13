using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneController : MonoBehaviour
{
    private bool isSettingsOpen = false;
    private bool isAudioOpen = false;

    private bool isTransitioning = false;

    [SerializeField] private float transitionDelay = 0.45f; // Adjust to match animation time

    private bool allowEscapeToggle = true;

    [SerializeField] private List<Button> settingsButtons;
    [SerializeField] private float settingsPanelDelay = 0.5f; // Animation duration

    //[Header("Fade Settings")]
    //public CanvasGroup canvasGroup; // Drag your splash canvas here
    //public float duration = 2f;
    //public float fadeDuration = 1f;

    void Update()
    {
        if (allowEscapeToggle && !isAudioOpen && Input.GetKeyDown(KeyCode.Escape) && !isTransitioning)
        {
            ToggleSettingsPanel();
        }
    }

    public void AudioOpen()
    {
        isAudioOpen = true;
    }

    public void AudioClose()
    {
        isAudioOpen = false;

    }
    public void ExitToMain()
    {
        allowEscapeToggle = false;
        CloseSettingsPanel();
    }


    public void ToggleSettingsPanel()
    {
        if (isSettingsOpen)
        {
            CloseSettingsPanel();
        }
        else
        {
            OpenSettingsPanel();
        }
    }

    public void OpenSettingsPanel()
    {
        if (UIManager.Instance == null || isTransitioning) return;

        isTransitioning = true;

        // Disable settings buttons before animation starts
        SetSettingsButtonsInteractable(false);

        UIManager.Instance.Open_PopupsAndPanels(UIType.Settings);
        Invoke(nameof(InvokePauseGame), transitionDelay);

        // Wait for animation to finish before allowing interaction
        Invoke(nameof(FinishSettingsPanelOpen), settingsPanelDelay);
    }

    public void CloseSettingsPanel()
    {
        if (UIManager.Instance == null || isTransitioning) return;

        isTransitioning = true;
        UIManager.Instance.Close_PopupsAndPanels(UIType.Settings);
        GameManager.Instance?.ResumeGame();
        Invoke(nameof(FinishTransition_Close), transitionDelay);
    }

    public void OpenAudioPanel()
    {
        if (UIManager.Instance == null || isTransitioning) return;

        isTransitioning = true;
        UIManager.Instance.Open_PopupsAndPanels(UIType.Audio);
        Invoke(nameof(InvokePauseGame), transitionDelay);
        Invoke(nameof(FinishTransition_Open), transitionDelay);
    }

    public void CloseAudioPanel()
    {
        if (UIManager.Instance == null || isTransitioning) return;

        isTransitioning = true;
        isAudioOpen = false;

        UIManager.Instance.Close_PopupsAndPanels(UIType.Audio);
        GameManager.Instance?.ResumeGame();
        Invoke(nameof(FinishTransition_Close), transitionDelay);
    }

    private void InvokePauseGame()
    {
        GameManager.Instance?.PauseGame();
    }

    private void InvokeResumeGame()
    {
        GameManager.Instance?.ResumeGame();
    }

    private void FinishTransition_Open()
    {
        isSettingsOpen = true;
        isTransitioning = false;
    }

    private void FinishTransition_Close()
    {
        isSettingsOpen = false;
        isTransitioning = false;
    }

    private void FinishSettingsPanelOpen()
    {
        isSettingsOpen = true;
        isTransitioning = false;
        SetSettingsButtonsInteractable(true);
    }

    private void SetSettingsButtonsInteractable(bool interactable)
    {
        foreach (var button in settingsButtons)
        {
            if (button != null)
                button.interactable = interactable;
        }
    }

}
