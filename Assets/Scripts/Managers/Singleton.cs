
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    singletonObject.name = typeof(T).Name;
                    _instance = singletonObject.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}
public class SingletonPersistent<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    singletonObject.name = typeof(T).Name;
                    _instance = singletonObject.AddComponent<T>();
                }
                
                // Make sure the singleton persists across scene loads
                DontDestroyOnLoad(_instance.gameObject);
            }
            
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            // If another instance already exists, destroy this one
            Destroy(gameObject);
        }
    }
}

