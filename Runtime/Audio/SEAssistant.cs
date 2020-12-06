namespace Origine
{

    using System;

    using UnityEngine;

    public class SEAssistant : AudioAssistant
    {
        private SEGroup _group;

        [SerializeField]
        private bool _isLoop = false;
        public bool IsLoop
        {
            get { return _isLoop; }
            set { _isLoop = value; }
        }

        public void Initialize(SEGroup audioGroup)
        {
            _group = audioGroup;
        }

        public override void Play()
        {
            Play(null);
        }

        public void Play(Action<AudioSource> callback)
        {
            if (_audioClip == null)
            {
                Debug.LogWarning(gameObject.name);
                callback?.Invoke(null);
                return;
            }

            _group.Play(_audioClip, _volumeRate, _delay, _pitch, _isLoop, callback: callback);
            if (_fadeInDuration > 0)
            {
                _group.FadeIn(_audioClip.name, _fadeInDuration);
            }
        }
    }
}