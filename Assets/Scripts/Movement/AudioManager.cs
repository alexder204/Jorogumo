using System.Collections;
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

        [Tooltip("Allow crossfade between this track and others")]
        public bool allowCrossfade = true;

        [Tooltip("Allow simple fade out/in transition (no overlap)")]
        public bool allowFade = true;
    }

    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;

    [Header("Scene Music")]
    [SerializeField] private SceneMusic[] sceneMusicArray;

    [Header("Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    private Dictionary<string, SceneMusic> sceneMusicMap;

    private AudioSource activeSource;
    private AudioSource inactiveSource;

    private Coroutine transitionCoroutine;

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
        sceneMusicMap = new Dictionary<string, SceneMusic>();
        foreach (var entry in sceneMusicArray)
        {
            if (!sceneMusicMap.ContainsKey(entry.sceneName))
            {
                sceneMusicMap.Add(entry.sceneName, entry);
            }
        }

        activeSource = musicSourceA;
        inactiveSource = musicSourceB;
        activeSource.volume = 1f;
        inactiveSource.volume = 0f;
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
        if (sceneMusicMap.TryGetValue(scene.name, out SceneMusic newSceneMusic))
        {
            PlayMusic(newSceneMusic);
        }
    }

    private void PlayMusic(SceneMusic newSceneMusic)
    {
        AudioClip newClip = newSceneMusic.musicClip;

        // If already playing this clip on active source, do nothing
        if (activeSource.clip == newClip && activeSource.isPlaying)
            return;

        // Stop any ongoing transition coroutine
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        // If currently no music playing, just play immediately
        if (activeSource.clip == null)
        {
            activeSource.clip = newClip;
            activeSource.volume = 1f;
            activeSource.Play();
            return;
        }

        // Check transition type based on settings (crossfade > fade > instant)
        if (newSceneMusic.allowCrossfade)
        {
            // Crossfade between sources
            transitionCoroutine = StartCoroutine(CrossfadeToNewClip(newClip, 2f));
        }
        else if (newSceneMusic.allowFade)
        {
            // Fade out then fade in on the same source
            transitionCoroutine = StartCoroutine(FadeOutInNewClip(newClip, 2f));
        }
        else
        {
            // Instant switch
            activeSource.Stop();
            activeSource.clip = newClip;
            activeSource.Play();
            activeSource.volume = 1f;
        }
    }

    private IEnumerator CrossfadeToNewClip(AudioClip newClip, float duration)
    {
        // Prepare inactive source
        inactiveSource.clip = newClip;
        inactiveSource.volume = 0f;
        inactiveSource.Play();

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            activeSource.volume = Mathf.Lerp(1f, 0f, t);
            inactiveSource.volume = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        // Swap active and inactive sources
        activeSource.Stop();
        AudioSource temp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = temp;

        activeSource.volume = 1f;
        inactiveSource.volume = 0f;
        transitionCoroutine = null;
    }

    private IEnumerator FadeOutInNewClip(AudioClip newClip, float duration)
    {
        float halfDuration = duration / 2f;

        // Fade out current music
        float timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;
            activeSource.volume = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        activeSource.Stop();
        activeSource.clip = newClip;
        activeSource.Play();

        // Fade in new music
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;
            activeSource.volume = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        transitionCoroutine = null;
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
