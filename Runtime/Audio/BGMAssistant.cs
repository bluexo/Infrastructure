namespace Origine
{

    using System;

    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class BGMAssistant : AudioAssistant
    {
        private BGMGroup _group;

        [SerializeField]
        private float _fadeOutDuration = 0;

        public float FadeOutDuration
        {
            get { return _fadeOutDuration; }
            set { _fadeOutDuration = value; }
        }
        
        [SerializeField]
        private bool _isLoop = true;
        public bool IsLoop
        {
            get { return _isLoop; }
            set { _isLoop = value; }
        }

        [SerializeField]
        private bool _isAutoStop = true;

        public bool IsAutoStop
        {
            get { return _isAutoStop; }
            set { _isAutoStop = value; }
        }

        protected override void Start()
        {
            base.Start();

            if (_isAutoStop)
                SceneManager.sceneUnloaded += OnUnloadedScene;
        }

        public void Initialize(BGMGroup audioGroup)
        {
            _group = audioGroup;
        }

        private void OnUnloadedScene(Scene scene)
        {
            SceneManager.sceneUnloaded -= OnUnloadedScene;
            if (_fadeOutDuration > 0)
            {
                _group.FadeOut(_fadeOutDuration);
            }
            else
            {
                _group.Stop();
            }
        }

        public override void Play()
        {
            if (_audioClip == null)
            {
                Debug.LogWarning(gameObject.name + "BGMAssistant_AudioClip");
                return;
            }
            _group.Play(_audioClip, _volumeRate, _delay, _pitch, _isLoop, allowsDuplicate: _fadeInDuration > 0);
            if (_fadeInDuration > 0)
            {
                _group.FadeIn(_audioClip.name, _fadeInDuration);
            }
        }
    }
}