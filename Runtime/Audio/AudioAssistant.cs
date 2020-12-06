using UnityEngine;

namespace Origine
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AudioAssistant : MonoBehaviour
    {
        [SerializeField]
        protected AudioClip _audioClip = null;
        public AudioClip AudioClip
        {
            get { return _audioClip; }
            set { _audioClip = value; }
        }

        [SerializeField]
        protected bool _isAutoPlay = false;
        public bool IsAutoPlay
        {
            get { return _isAutoPlay; }
            set { _isAutoPlay = value; }
        }

        [SerializeField]
        protected float _volumeRate = 1, _delay = 0, _pitch = 1, _fadeInDuration = 0;
        public float VolumeRate
        {
            get { return _volumeRate; }
            set { _volumeRate = value; }
        }

        public float Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        public float Pitch
        {
            get { return _pitch; }
            set { _pitch = value; }
        }

        public float FadeInDuration
        {
            get { return _fadeInDuration; }
            set { _fadeInDuration = value; }
        }

        protected virtual void Start()
        {
            if (_isAutoPlay)
                Play();
        }

        public abstract void Play();

    }
}