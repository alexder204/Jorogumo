using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
    }

    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;

    [Header("Scene Music")]
    [SerializeField] private SceneMusic[] sceneMusicArray;

    [Header("Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    private Dictionary<string, AudioClip> sceneMusicMap;

    void Awake()
    {
        // Singleton setup
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

        // Build scene-to-music mapping
        sceneMusicMap = new Dictionary<string, AudioClip>();
        foreach (var entry in sceneMusicArray)
        {
            if (!sceneMusicMap.ContainsKey(entry.sceneName))
            {
                sceneMusicMap.Add(entry.sceneName, entry.musicClip);
            }
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (sceneMusicMap.TryGetValue(scene.name, out AudioClip newClip))
        {
            PlayMusic(newClip);
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
    }

    // Optional: Volume setters for menus or sliders
    public void SetMasterVolume(float volume)
    {
        mainMixer.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        mainMixer.SetFloat("SFXVolume", volume);
    }
}
