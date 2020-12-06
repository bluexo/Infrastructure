namespace Origine
{

    using System;
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;

    public partial class BGMGroup
    {
        public void FadeOut(string audioPath, 
            float fadeOutDuration = 1f,
            float volumeRate = 1, 
            float delay = 0, 
            float pitch = 1,
            bool isLoop = true, 
            Action callback = null)
        {
            if (!IsPlaying)
            {
                Play(audioPath, volumeRate, delay, pitch, isLoop);
                return;
            }

            FadeOut(fadeOutDuration, () =>
            {
                Play(audioPath, volumeRate, delay, pitch, isLoop);
                callback?.Invoke();
            });
        }

        public void FadeIn(string audioPath, 
            float fadeInDuration = 1f, 
            float volumeRate = 1,
            float delay = 0,
            float pitch = 1,
            bool isLoop = true, 
            Action callback = null)
        {
            Stop();
            Play(audioPath, volumeRate, delay, pitch, isLoop);
            FadeIn(audioPath, fadeInDuration, callback);
        }

        public void FadeOutAndFadeIn(string audioPath, 
            float fadeOutDuration = 1f, 
            float fadeInDuration = 1f, 
            float volumeRate = 1, 
            float delay = 0, 
            float pitch = 1, 
            bool isLoop = true, 
            Action callback = null)
        {
            if (!IsPlaying)
            {
                FadeIn(audioPath, fadeInDuration, volumeRate, delay, pitch, isLoop, callback);
                return;
            }

            FadeOut(fadeOutDuration, () =>
            {
                FadeIn(audioPath, fadeInDuration, volumeRate, delay, pitch, isLoop, callback);
            });
        }

        public void CrossFade(string audioPath,
            float fadeDuration = 1f,
            float volumeRate = 1,
            float delay = 0,
            float pitch = 1, 
            bool isLoop = true, 
            Action callback = null)
        {
            if (GetCurrentAudioNames().Count >= AudioPlayerCount)
            {
                Debug.LogWarning("Audio Player Num Not Enough！");
            }

            foreach (var currentAudioName in GetCurrentAudioNames())
            {
                FadeOut(currentAudioName, fadeDuration);
            }

            Play(audioPath, volumeRate, delay, pitch, isLoop, allowsDuplicate: true);
            FadeIn(audioPath, fadeDuration, callback);
        }

    }

}