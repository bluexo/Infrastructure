namespace Origine
{
    using System;
    using System.Collections;

    using UnityEngine;

    public partial class BGMGroup : AudioGroup
    {
        public void Play(AudioClip audioClip,
            float volumeRate = 1,
            float delay = 0,
            float pitch = 1,
            bool isLoop = true,
            bool allowsDuplicate = false,
            Action<AudioSource> callback = null)
        {
            if (!allowsDuplicate)
                Stop();
            RunPlayer(audioClip, volumeRate, delay, pitch, isLoop, callback);
        }

        public void Play(string audioPath,
            float volumeRate = 1,
            float delay = 0,
            float pitch = 1,
            bool isLoop = true,
            bool allowsDuplicate = false,
            Action<AudioSource> callback = null)
        {
            if (!allowsDuplicate)
                Stop();
            StartCoroutine(RunPlayer(audioPath, volumeRate, delay, pitch, isLoop, callback));
        }

    }
}