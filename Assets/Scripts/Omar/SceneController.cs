using UnityEngine;

public class SceneController : MonoBehaviour
{
    public void TransitionPressed(string sceneName)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TransitionManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("GameManager or TransitionManager is missing!");
        }
    }
}
