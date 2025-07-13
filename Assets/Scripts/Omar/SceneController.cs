using UnityEngine;

public class SceneController : MonoBehaviour
{
    void Update()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

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
