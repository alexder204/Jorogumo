using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BackgroundMusic : MonoBehaviour
{
    private AudioSource source;
    private void Start()
    {
        source = GetComponent<AudioSource>();
        source.volume = 0f;
        StartCoroutine(FadeIn(true, source, 2f, 0.2f));
        StartCoroutine(FadeIn(false, source, 2f, 0f));
    }
    private void Update()
    {
        if (!source.isPlaying)
        {
            source.Play();
            StartCoroutine(FadeIn(true, source, 2f, 0.2f));
            StartCoroutine(FadeIn(false, source, 2f, 0f));
        }
    }
    public IEnumerator FadeIn(bool fadeIn, AudioSource source, float duration, float targetVolume)
    {
        if (!fadeIn)
        {
            double lengthofSource = (double)source.clip.samples / source.clip.frequency;
            yield return new WaitForSecondsRealtime((float)(lengthofSource - duration));
        }
        float time = 0f;
        float startVOl = source.volume;
        while (time < duration)
        {
            string fadeSituation = fadeIn ? "fadeIn" : "fadeOut";
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVOl, targetVolume, time / duration);
            yield return null;
        }
        yield break;
    }
}