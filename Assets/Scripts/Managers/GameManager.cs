using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : SingletonPersistent<GameManager>
{
    public GameState state;
    public static event Action<GameState> OnGameStateChanged;
    // public PlayerCore PlayerCore { get; set;}
    public TransitionManager TransitionManager;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDestroy()
    {
        // Clean up event subscriptions
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Start()
    {
        //UpdateGameState(GameState.MainMenu);
    }

    public void UpdateGameState(GameState newState)
    {
        state = newState;
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.GamePlay:
                Time.timeScale = 1f;
                break;
            case GameState.GameOver:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnGameStateChanged?.Invoke(newState);
    }

   // public void LeveCompleted()
    //{

    //}

    //public void SaveGame()
    //{
        // PlayerCore ??= FindObjectOfType<PlayerCore>();

        //if (PlayerCore != null)
        //_saveManager.SerializeJson(PlayerCore.playerStats);

    //}
    //public void PauseGame()

    public void PauseGame()
    {
        UpdateGameState(GameState.Paused);
    }

    public void ResumeGame()
    {
        UpdateGameState(GameState.GamePlay);
    }
    public void GameOver()
    {
        UpdateGameState(GameState.GameOver);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        // Handle scene-specific initialization
        if (scene.name.Contains("Level_"))
        {
            UpdateGameState(GameState.GamePlay);
            AudioManager.Instance.PlayMusic("Game_BGM");

        }
        else if (scene.name == "MainMenu")
        {
            UpdateGameState(GameState.MainMenu);
            AudioManager.Instance.PlayMusic("MainMenu_BGM");
        }

        else if (scene.name == "Stage1")
        {
            UpdateGameState(GameState.Stage1);
            AudioManager.Instance?.PlayBGM_Stages(GameState.Stage1.ToString());
        }

        else if (scene.name == "Stage2")
        {
            UpdateGameState(GameState.Stage2);
            AudioManager.Instance?.PlayBGM_Stages(GameState.Stage2.ToString());
        }

    }

    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"Scene unloaded: {scene.name}");
    }



}

public enum GameState
{
    Init,
    MainMenu,
    Stage1,
    Stage2,
    Paused,
    GamePlay,
    GameOver
}