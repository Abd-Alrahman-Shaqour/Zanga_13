using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    #region Instance | Singleton
    private static AudioManager _instance;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;


    public static AudioManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<AudioManager>();

			return _instance;
		}
	}

    private void DontDestroy() 
    { 
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            _instance = this; 
            DontDestroyOnLoad(this.gameObject);
        } 
    }
    #endregion --- Instance | Singleton ---

#region Consts
    public static string SOUND_SETTINGS_KEY = "soundsSettings";
    public static string MUSIC_SETTINGS_KEY = "musicSettings";

    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";
    #endregion --- Consts ---

    #region Vars
    [Header("Main Components"), Space]
    [SerializeField] AudioSource BGM_AudioSource;
    [SerializeField] AudioSource SFX_AudioSource;

    [Header("Settings"), Space]
    [SerializeField] bool isSoundActive;
    [SerializeField] bool isMusicActive;

    [Header("SFX Parameters")]
    [SerializeField] List<AudioElement> SFX_AudioElements;

    [Header("BGM Parameters")]
    [SerializeField] bool isBGMTransitioning;
    [SerializeField] float transitionDuration = 1;
    [SerializeField] float maxVolume = 1;
    [SerializeField]  List<AudioElement> BGM_AudioElements;
    
#endregion --- Vars ---

    void Awake()
    {
        DontDestroy();
    }



    public void SetMusicVolume(float value)
    {
        float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("MusicVolume", db);
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
    }

    public void SetSFXVolume(float value)
    {
        float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("SFXVolume", db);
        PlayerPrefs.SetFloat(SFX_KEY, value);
    }

    public void LoadVolumeSettings()
    {
        float music = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        SetMusicVolume(music); // <- Applies AND saves (again)
        SetSFXVolume(sfx);
    }

    void Start()
    {
        InitSavedSettings();
        LoadVolumeSettings();
        // PlayMusic(BGM_AudioElements.FirstOrDefault().audioName);
    }

    public AudioElement GetSFX_ByName(string name)
    {
        if(SFX_AudioElements.Count == 0)
            return null;

        return SFX_AudioElements.FirstOrDefault(audioElement => audioElement.audioName == name);
    }

    public AudioElement GetBGM_ByName(string name)
    {
        if(BGM_AudioElements.Count == 0)
            return null;

        return BGM_AudioElements.FirstOrDefault(audioElement => audioElement.audioName == name);
    }

    public void PlaySFX(string _name, float _volume = 1.0f, bool _isOneShot = true)
    {
        Debug.Log("SFX name: " + _name);

        if(!isSoundActive)
        {
            return;
        }

        AudioElement audioElement = GetSFX_ByName(_name);

        if (audioElement == null)
            return;

        if(_isOneShot)
        {
            SFX_AudioSource.PlayOneShot(audioElement.audioClip, _volume);
        }
        else
        {
            SFX_AudioSource.clip = audioElement.audioClip;
            SFX_AudioSource.volume = _volume;
            SFX_AudioSource.Play();
        }
    }

    public void PlayMusic(string _trackName, bool _forceTransition = false)
    {
        Debug.Log($"PlayMusic called, Track name: {_trackName}");

        if(!isMusicActive)
        {
            return;
        }

        if(_forceTransition)
            isBGMTransitioning = false;

        if(isBGMTransitioning)
            return;
        
        isBGMTransitioning = true;

        AudioElement newTrack = GetBGM_ByName(_trackName);

        Debug.Log($"newTrack null?: {newTrack == null}");

        if(newTrack == null)
        {
            Debug.LogWarning("AudioElement not found or is invalid.");
            isBGMTransitioning = false;
            return;
        }

        if(BGM_AudioSource.isPlaying)
        {
            StartCoroutine(TransitionToNewTrack(newTrack, transitionDuration));
        }
        else
        {
            BGM_AudioSource.clip = newTrack.audioClip;
            BGM_AudioSource.volume = 0;
            BGM_AudioSource.Play();
            BGM_AudioSource.DOFade(maxVolume, transitionDuration).OnComplete(()=>
            {
                isBGMTransitioning = false;
            });
        }
    }

    public void FadeInMusic()
    {
        BGM_AudioSource.DOFade(maxVolume, transitionDuration);
    }

    public void FadeOutMusic()
    {
        BGM_AudioSource.DOFade(0, transitionDuration);
    }

    private IEnumerator TransitionToNewTrack(AudioElement newTrack, float fadeDuration)
    {
        yield return BGM_AudioSource.DOFade(0, fadeDuration / 2).WaitForCompletion();
        
        BGM_AudioSource.clip = newTrack.audioClip;
        BGM_AudioSource.Play();

        yield return BGM_AudioSource.DOFade(maxVolume, fadeDuration / 2).WaitForCompletion();

        isBGMTransitioning = false;
    }

    void InitSavedSettings()
    {
        isSoundActive = Get_SoundSettings();
        isMusicActive = Get_MusicSettings();

        Set_SoundSettings(Get_SoundSettings());
        Set_MusicSettings(Get_MusicSettings());
    }

    public void Set_SoundSettings(bool _state)
    {
        PlayerPrefs.SetInt(SOUND_SETTINGS_KEY, _state ? 1 : 0);
        isSoundActive = _state;
    }

    public void Set_MusicSettings(bool _state)
    {
        PlayerPrefs.SetInt(MUSIC_SETTINGS_KEY, _state ? 1 : 0);
        isMusicActive = _state;

        //BGM_AudioSource.volume = isMusicActive ? maxVolume : 0;
        SetMusicVolume(isMusicActive ? PlayerPrefs.GetFloat(MUSIC_KEY, 1f) : 0.0001f);

    }

    public bool Get_SoundSettings()
    {
        return PlayerPrefs.GetInt(SOUND_SETTINGS_KEY, 1) == 1 ? true : false;
    }

    public bool Get_MusicSettings()
    {
        return PlayerPrefs.GetInt(MUSIC_SETTINGS_KEY, 1) == 1 ? true : false;
    }

    public void PlayWalk()
    {
        int count = SFX_AudioElements.Where(x => x.audioName.Contains("Walk")).Count();

        PlaySFX("Walk_" + UnityEngine.Random.Range(1, count), 0.35f, true);
    }

    public Coroutine BGMCoroutine = null;

    public void PlayBGMMusicForStages(string _stageName)
    {
        if(BGMCoroutine != null)
        {
            StopCoroutine(BGMCoroutine);
            BGMCoroutine = null;
        }

        BGMCoroutine = StartCoroutine(PlayBGM_Stages(_stageName));
    }

    public IEnumerator PlayBGM_Stages(string _stageName)
    {
        int count = BGM_AudioElements.Where(x => x.audioName.Contains(_stageName)).Count();

        for(int i = 0; i < count; i++)
        {
            PlayMusic(_stageName + i, true);
            yield return new WaitForSeconds(BGM_AudioSource.clip.length);
        }

        PlayBGMMusicForStages(_stageName);
    }

    [Serializable]
    public class AudioElement
    {
        public string audioName;
        public AudioClip audioClip;
    }

// #if (UNITY_EDITOR)   
//     [CustomEditor(typeof(AudioManager))]
//     public class CustomInspector : Editor
//     {
//         public override void OnInspectorGUI()
//         {
//             DrawDefaultInspector();
    
//             AudioManager audioManager = (AudioManager) target;
    
//             if(GUILayout.Button("music_1"))
//             {
//                 audioManager.PlayMusic("1");
//             }

//             if(GUILayout.Button("music_2"))
//             {
//                 audioManager.PlayMusic("2");
//             }
//         }
//     }
// #endif
}