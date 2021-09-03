namespace Origine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.SceneManagement;

    public enum AudioCacheType
    {
        None,
        Used
    }

    /// <summary>
    /// 音频组
    /// </summary>
    public abstract class AudioGroup : MonoBehaviour
    {
        private AudioCacheType _cacheType;
        private Dictionary<string, AudioClip> _audioClipDict = new Dictionary<string, AudioClip>();
        protected readonly List<AudioPlayer> _audioPlayerList = new List<AudioPlayer>();
        private int _nextAudioPlayerNo = 0;
        public float Volume => _baseVolume;

        public virtual int AudioPlayerCount { get; protected set; }
        public const float DEFAULTVOLUME = .5f;
        private float _baseVolume = DEFAULTVOLUME;

        protected virtual void Awake()
        {
        }

        public virtual IEnumerator InitializeAsync(AudioManagerSetting config)
        {
            AudioPlayerCount = config.AudioPlayerCount;
            for (int i = 0; i < AudioPlayerCount; i++)
                _audioPlayerList.Add(new AudioPlayer(gameObject.AddComponent<AudioSource>()));
            yield return LoadAudioClip(config.PreloadAudioPath, config.CacheType, config.IsReleaseCache);
            ChangeBaseVolume(PlayerPrefs.GetFloat(GetType().FullName, config.BaseVolume));
        }

        protected IEnumerator LoadAudioClip(string preloadPath, AudioCacheType cacheType, bool isReleaseCache)
        {
            if (!string.IsNullOrWhiteSpace(preloadPath))
            {
                var handle = Addressables.LoadAssetsAsync<AudioClip>(preloadPath, audio => _audioClipDict[preloadPath + audio.name] = audio);
                yield return handle;
            }

            _cacheType = cacheType;
            if (_cacheType == AudioCacheType.Used && isReleaseCache)
                SceneManager.sceneUnloaded += (scene) => _audioClipDict.Clear();
        }

        public void Update()
        {
            for (var i = 0; i < _audioPlayerList.Count; i++)
            {
                var audioPlayer = _audioPlayerList[i];
                if (audioPlayer.CurrentState != AudioPlayer.State.Wait)
                    audioPlayer.Update();
            }
        }

        public void ChangeBaseVolume(float baseVolume)
        {
            _baseVolume = baseVolume;
            PlayerPrefs.SetFloat(GetType().FullName, _baseVolume);
            _audioPlayerList.Where(player => player.CurrentState != AudioPlayer.State.Wait).ToList()
                .ForEach(player => player.ChangeVolume(_baseVolume));
        }

        protected bool FilterPlayer(AudioPlayer player, string audioName)
        {
            if (_audioClipDict.ContainsKey(audioName))
                audioName = _audioClipDict[audioName].name;

            return !string.IsNullOrWhiteSpace(player.CurrentAudioName) && string.Equals(player.CurrentAudioName, audioName, StringComparison.OrdinalIgnoreCase);
        }

        public List<string> GetCurrentAudioNames() => _audioPlayerList
                .Where(player => player.CurrentState != AudioPlayer.State.Wait)
                .Select(player => player.CurrentAudioName)
                .ToList();

        public bool IsPlaying => _audioPlayerList.Any(p => p.CurrentState == AudioPlayer.State.Playing);

        protected void RunPlayer(AudioClip audioClip,
            float volumeRate,
            float delay,
            float pitch,
            bool isLoop,
            Action<AudioSource> callback = null) => GetNextAudioPlayer().Play(audioClip, _baseVolume, volumeRate, delay, pitch, isLoop, callback);

        protected IEnumerator RunPlayer(string audioName,
            float volumeRate,
            float delay,
            float pitch,
            bool isLoop,
            Action<AudioSource> callback = null)
        {
            AudioClip current;
            if (!_audioClipDict.ContainsKey(audioName))
            {
                var handle = Addressables.LoadAssetAsync<AudioClip>(audioName);
                yield return handle;
                current = handle.Result;
                if (_cacheType == AudioCacheType.Used)
                    _audioClipDict[audioName] = current;
            }
            else
            {
                current = _audioClipDict[audioName];
            }
            RunPlayer(current, volumeRate, delay, pitch, isLoop, callback);
        }

        private AudioPlayer GetNextAudioPlayer()
        {
            var audioPlayer = _audioPlayerList[_nextAudioPlayerNo];

            _nextAudioPlayerNo++;
            if (_nextAudioPlayerNo >= _audioPlayerList.Count)
                _nextAudioPlayerNo = 0;

            return audioPlayer;
        }

        public void Stop(string audioName)
        {
            if (string.IsNullOrWhiteSpace(audioName)) return;

            _audioPlayerList.ForEach(player =>
            {
                if (!FilterPlayer(player, audioName)) return;
                player.Stop();
            });
        }

        public void Stop() => _audioPlayerList.ForEach(player => player.Stop());

        public void Fade(string audioName, float duration, float from, float to, Action callback)
          => _audioPlayerList.ForEach(player =>
           {
               if (!FilterPlayer(player, audioName)) return;
               player.Fade(duration, from, to, callback);
           });

        public void FadeOut(string audioName, float duration = 1f, Action callback = null) => Fade(audioName, duration, 1, 0, callback);

        public void FadeIn(string audioName, float duration = 1f, Action callback = null) => Fade(audioName, duration, 0, 1, callback);

        public void Fade(float duration, float from, float to, Action callback) => _audioPlayerList.ForEach(player => player.Fade(duration, from, to, callback));

        public void FadeOut(float duration = 1f, Action callback = null) => Fade(duration, 1, 0, callback);

        public void FadeIn(float duration = 1f, Action callback = null) => Fade(duration, 0, 1, callback);

        public void Pause(string audioName)
            => _audioPlayerList.ForEach(player =>
            {
                if (!FilterPlayer(player, audioName)) return;
                player.Pause();
            });

        public void SetPause(bool pause)
        {
            if (pause) Pause();
            else Resume();
        }

        public void Switch()
        {
            if (IsPlaying) Pause();
            else Resume();
        }

        public void Pause() => _audioPlayerList.ForEach(player => player.Pause());

        public void Resume(string audioName)
           => _audioPlayerList.ForEach(player =>
           {
               if (!FilterPlayer(player, audioName)) return;
               player.Resume();
           });

        public void Resume() => _audioPlayerList.ForEach(player => player.Resume());
    }
}