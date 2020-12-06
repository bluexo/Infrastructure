namespace Origine
{

    using System;
    using System.Collections;

    using UnityEngine;

    public class SEGroup : AudioGroup
    {
        [SerializeField]
        private bool _shouldAdjustVolumeRate = true;

        public override IEnumerator InitializeAsync(AudioManagerSetting setting)
        {
            yield return base.InitializeAsync(setting);
            _shouldAdjustVolumeRate = setting.ShouldAdjustVolumeRate;
            ChangeBaseVolume(setting.BaseVolume);
        }

        public void Play(AudioClip audioClip,
            float volumeRate = 1,
            float delay = 0,
            float pitch = 1,
            bool isLoop = false,
            Action<AudioSource> callback = null)
        {
            volumeRate = AdjustVolumeRate(volumeRate, audioClip.name);
            if (volumeRate <= 0) return;
            RunPlayer(audioClip, volumeRate, delay, pitch, isLoop, callback);
        }

        public void Play(string audioPath,
            float volumeRate = 1,
            float delay = 0,
            float pitch = 1,
            bool isLoop = false,
            Action<AudioSource> callback = null)
        {
            volumeRate = AdjustVolumeRate(volumeRate, audioPath);
            if (volumeRate <= 0) return;
            StartCoroutine(RunPlayer(audioPath, volumeRate, delay, pitch, isLoop, callback));
        }

        private float AdjustVolumeRate(float volumeRate, string audioName)
        {
            if (!_shouldAdjustVolumeRate)
                return volumeRate;

            var targetAudioPlayers = _audioPlayerList.FindAll(player => player.CurrentAudioName == audioName);
            if (targetAudioPlayers.Count == 0)
                return volumeRate;

            foreach (var targetAudioPlayer in _audioPlayerList.FindAll(player => player.CurrentAudioName == audioName))
            {
                if (targetAudioPlayer.CurrentVolume <= 0)
                    continue;

                float playedTime = targetAudioPlayer.PlayedTime;
                if (targetAudioPlayer.CurrentState == AudioPlayer.State.Delay)
                    playedTime += targetAudioPlayer.ElapsedDelay;

                if (playedTime <= 0.025f)
                    return 0;
                else if (playedTime <= 0.05f)
                    volumeRate *= 0.8f;
                else if (playedTime <= 0.1f)
                    volumeRate *= 0.9f;
            }

            return volumeRate;
        }
    }
}