namespace Origine
{

    using System;

    using UnityEngine;

    public class AudioPlayer
    {
        private readonly AudioSource _audioSource;
        public float PlayedTime => _audioSource.time;
        public string CurrentAudioName => !_audioSource || !_audioSource.clip ? "" : _audioSource.clip.name;

        private Action<AudioSource> _callback;

        public enum State
        {
            Wait,
            Delay, 
            Playing, 
            Pause, 
            Fading
        }

        public State CurrentState { get; private set; } = State.Wait;

        private float _baseVolume, _volumeRate;
        public float CurrentVolume => _baseVolume * _volumeRate;

        private float _initialDelay, _currentDelay;
        public float ElapsedDelay => _initialDelay - _currentDelay;

        private float _fadeProgress, _fadeDuration, _fadeFrom, _fadeTo;
        private Action _fadeCallback;

        public AudioPlayer(AudioSource audioSource)
        {
            _audioSource = audioSource;
            _audioSource.playOnAwake = false;
        }

        public void Update()
        {
            if (CurrentState == State.Playing && !_audioSource.isPlaying && Mathf.Approximately(_audioSource.time, 0))
                Finish();
            else if (CurrentState == State.Delay)
                Delay();
            else if (CurrentState == State.Fading)
                Fade();
        }

        private void Delay()
        {
            _currentDelay -= Time.deltaTime;
            if (_currentDelay > 0)
                return;

            _audioSource.Play();

            if (_fadeDuration > 0)
            {
                CurrentState = State.Fading;
                Update();
            }
            else
            {
                CurrentState = State.Playing;
            }
        }

        private void Fade()
        {
            _fadeProgress += Time.deltaTime;
            float timeRate = Mathf.Min(_fadeProgress / _fadeDuration, 1);

            _audioSource.volume = GetVolume() * (_fadeFrom * (1 - timeRate) + _fadeTo * timeRate);

            if (timeRate < 1)
                return;

            if (_fadeTo <= 0)
                Finish();
            else
                CurrentState = State.Playing;

            _fadeCallback?.Invoke();
        }

        public void ChangeVolume(float baseVolume)
        {
            _baseVolume = baseVolume;
            _audioSource.volume = GetVolume();
        }

        public void ChangeVolumeRate(float volumeRate)
        {
            _volumeRate = volumeRate;
            _audioSource.volume = GetVolume();
        }

        private float GetVolume()
        {
            return _baseVolume * _volumeRate;
        }

        public void Play(AudioClip audioClip,
            float baseVolume,
            float volumeRate,
            float delay,
            float pitch,
            bool isLoop,
            Action<AudioSource> callback = null)
        {
            if (CurrentState != AudioPlayer.State.Wait)
                Stop();
            _audioSource.Stop();

            _volumeRate = volumeRate;
            ChangeVolume(baseVolume);

            _initialDelay = delay;
            _currentDelay = _initialDelay;

            _audioSource.pitch = pitch;
            _audioSource.loop = isLoop;
            _callback = callback;

            _audioSource.clip = audioClip;

            CurrentState = _currentDelay > 0 ? State.Delay : State.Playing;
            if (CurrentState == State.Playing)
                _audioSource.Play();

            if (_audioSource.loop)
                return;

            if (CurrentState == State.Pause)
                Pause();
        }

        public void Stop()
        {
            Finish();
            _callback = null;
        }

        private void Finish()
        {
            CurrentState = State.Wait;

            _audioSource.Stop();
            _audioSource.clip = null;

            _initialDelay = 0;
            _currentDelay = 0;
            _fadeDuration = 0;

            _callback?.Invoke(_audioSource);
        }

        public void Pause()
        {
            if (CurrentState == State.Playing || CurrentState == State.Fading)
                _audioSource.Pause();

            CurrentState = State.Pause;
        }

        public void Resume()
        {
            if (CurrentState != State.Pause)
                return;

            if (_audioSource.clip == null)
            {
                CurrentState = State.Wait;
            }
            else if (_currentDelay > 0)
            {
                CurrentState = State.Delay;
            }
            else
            {
                _audioSource.UnPause();
                CurrentState = _fadeDuration > 0 ? State.Fading : State.Playing;
            }
        }

        public void Fade(float duration, float from, float to, Action callback = null)
        {
            if (CurrentState != State.Playing && CurrentState != State.Delay && CurrentState != State.Fading)
                return;

            _fadeProgress = 0;
            _fadeDuration = duration;
            _fadeFrom = from;
            _fadeTo = to;
            _fadeCallback = callback;

            if (CurrentState == State.Playing)
                CurrentState = State.Fading;

            if (CurrentState == State.Fading)
                Update();
        }
    }
}