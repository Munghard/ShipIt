
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class AudioManager
    {
        public static AudioSource Play(string clipName)
        {
            // Create a temporary GameObject to play the audio
            GameObject tempGO = new GameObject("TempAudio");
            AudioSource audioSource = tempGO.AddComponent<AudioSource>();

            var clip = Resources.Load<AudioClip>($"Audio/{clipName}");
            audioSource.clip = clip;
            audioSource.Play();

            // Destroy the GameObject after the clip finishes
            Object.Destroy(tempGO, clip.length);

            return audioSource;
        }
    }
}
