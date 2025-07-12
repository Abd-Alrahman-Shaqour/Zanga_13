using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Feedbacks;
using System.Collections;

public class CheckpointManager : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    public MMF_Player fadeOutFeedback;
    public MMF_Player fadeInFeedback;
    public float respawnDelay = 1f;
    
    [Header("Current Checkpoint")]
    public Vector3 currentCheckpointPosition = Vector3.zero;
    public Quaternion currentCheckpointRotation = Quaternion.identity;
    
    [SerializeField]private GameObject player;
    private string currentSceneName;
    private bool isRespawning = false;
    
    public static CheckpointManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        currentSceneName = SceneManager.GetActiveScene().name;
    }
    
    void Start()
    {
        LoadCheckpoint();
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        LoadCheckpoint();
    }
    
    
    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        currentCheckpointPosition = position;
        currentCheckpointRotation = rotation;
        SaveCheckpoint();
        Debug.Log($"Checkpoint set at: {position}");
    }
    
    public void OnPlayerDeath()
    {
        if (isRespawning) return;
        
        StartCoroutine(RespawnSequence());
    }
    
    IEnumerator RespawnSequence()
    {
        isRespawning = true;
        

        
            // Fade in
        if (fadeInFeedback != null)
        {
            fadeInFeedback.PlayFeedbacks();
            yield return new WaitForSeconds(fadeInFeedback.TotalDuration);
        }
        if (player != null)
        {
            player.SetActive(false);
        }
        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);
        
        // Respawn player
        RespawnPlayer();
        
         if (fadeOutFeedback != null)
        {
            fadeOutFeedback.PlayFeedbacks();
            yield return new WaitForSeconds(fadeOutFeedback.TotalDuration);
        }
        
        // Re-enable player controls
        if (player != null)
        {
            player.SetActive(true);
        }
        
        isRespawning = false;
    }
    
    void RespawnPlayer()
    {
        if (player != null)
        {
            // Reset player position and rotation
            player.transform.position = currentCheckpointPosition;
            player.transform.rotation = currentCheckpointRotation;
            
        }
        else
        {
            Debug.LogError("Player not found for respawn!");
        }
    }
    
    void SaveCheckpoint()
    {
        string key = "Checkpoint_" + currentSceneName;
        PlayerPrefs.SetFloat(key + "_PosX", currentCheckpointPosition.x);
        PlayerPrefs.SetFloat(key + "_PosY", currentCheckpointPosition.y);
        PlayerPrefs.SetFloat(key + "_PosZ", currentCheckpointPosition.z);
        PlayerPrefs.SetFloat(key + "_RotX", currentCheckpointRotation.x);
        PlayerPrefs.SetFloat(key + "_RotY", currentCheckpointRotation.y);
        PlayerPrefs.SetFloat(key + "_RotZ", currentCheckpointRotation.z);
        PlayerPrefs.SetFloat(key + "_RotW", currentCheckpointRotation.w);
        PlayerPrefs.Save();
    }
    
    void LoadCheckpoint()
    {
        string key = "Checkpoint_" + currentSceneName;
        
        if (PlayerPrefs.HasKey(key + "_PosX"))
        {
            float x = PlayerPrefs.GetFloat(key + "_PosX");
            float y = PlayerPrefs.GetFloat(key + "_PosY");
            float z = PlayerPrefs.GetFloat(key + "_PosZ");
            currentCheckpointPosition = new Vector3(x, y, z);
            
            float rotX = PlayerPrefs.GetFloat(key + "_RotX");
            float rotY = PlayerPrefs.GetFloat(key + "_RotY");
            float rotZ = PlayerPrefs.GetFloat(key + "_RotZ");
            float rotW = PlayerPrefs.GetFloat(key + "_RotW");
            currentCheckpointRotation = new Quaternion(rotX, rotY, rotZ, rotW);
            
            Debug.Log($"Loaded checkpoint for scene {currentSceneName}: {currentCheckpointPosition}");
        }
        else
        {
            Debug.Log($"No checkpoint found for scene {currentSceneName}");
        }
    }
    
    public void ClearCheckpoint()
    {
        string key = "Checkpoint_" + currentSceneName;
        PlayerPrefs.DeleteKey(key + "_PosX");
        PlayerPrefs.DeleteKey(key + "_PosY");
        PlayerPrefs.DeleteKey(key + "_PosZ");
        PlayerPrefs.DeleteKey(key + "_RotX");
        PlayerPrefs.DeleteKey(key + "_RotY");
        PlayerPrefs.DeleteKey(key + "_RotZ");
        PlayerPrefs.DeleteKey(key + "_RotW");
        PlayerPrefs.Save();
    }
}


