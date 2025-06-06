using UnityEngine;
using System.Collections;

public class PlayAudioFadeTrigger2D : MonoBehaviour
{
    public AudioSource audioSource;
    public float fadeDuration = 0.5f;
    public float targetVolume = 1f;

    private Coroutine currentFade;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (audioSource != null)
            {
                if (currentFade != null)
                    StopCoroutine(currentFade);

                audioSource.volume = 0f;
                audioSource.Play();
                currentFade = StartCoroutine(FadeAudio(targetVolume));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                if (currentFade != null)
                    StopCoroutine(currentFade);

                currentFade = StartCoroutine(FadeAudio(0f, stopAfter: true));
            }
        }
    }

    private IEnumerator FadeAudio(float target, bool stopAfter = false)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, target, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = target;

        if (stopAfter && target == 0f)
            audioSource.Stop();
    }
}
