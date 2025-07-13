using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class TransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    public CanvasGroup fadeCanvasGroup;
    [Range(0.5f, 5f)] public float fadeDuration = 2.5f;
    private static TransitionManager instance;

    private void Awake()
    {
        // Singleton pattern to persist TransitionManager
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 1f;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Init")
        {
            StartCoroutine(DelayBeforeLoad("MainMenu"));
        }
    }

    private IEnumerator DelayBeforeLoad(string sceneName)
    {
        yield return new WaitForSeconds(2f); // adjust to how long you want the splash to show
        LoadScene(sceneName);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Automatically fade out after a scene is loaded
        StartCoroutine(FadeOut());
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return FadeIn();

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        // Fade-out will happen automatically after scene is loaded (via OnSceneLoaded)
        yield return new WaitForSeconds(0.2f);
        op.allowSceneActivation = true;
    }

    public IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(0.5f);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1;
            fadeCanvasGroup.blocksRaycasts = false;

            fadeCanvasGroup.DOFade(0, fadeDuration).SetEase(Ease.InOutSine).SetUpdate(true);
        }

        yield return new WaitForSeconds(fadeDuration);
    }

    private IEnumerator FadeIn()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0;
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.blocksRaycasts = true;

            yield return fadeCanvasGroup.DOFade(1, fadeDuration).SetEase(Ease.InOutSine).SetUpdate(true).WaitForCompletion();
        }
    }
}
