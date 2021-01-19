using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Origine
{
    public class AudioManager : GameModule, IAudioManager
    {
        readonly Dictionary<Type, AudioGroup> groups = new Dictionary<Type, AudioGroup>();
        readonly GameObject root = new GameObject(nameof(AudioManager));

        public AudioManager(GameContext gameContext)
        {
            GameObject.DontDestroyOnLoad(root);
        }

        public IEnumerator InitializeAsync(string settingPath)
        {
            var types = AssemblyCollection.GetTypes(t => t.IsSubclassOf(typeof(AudioGroup)));
            foreach (var type in types)
            {
                var child = new GameObject(type.Name, type);
                child.transform.SetParent(root.transform);
                var handle = Addressables.LoadAssetAsync<AudioManagerSetting>($"{settingPath}/{type.Name}.asset");
                yield return handle;
                var group = child.GetComponent<AudioGroup>();
                yield return group.InitializeAsync(handle.Result);
                groups.Add(type, child.GetComponent<AudioGroup>());
            }
        }

        public void FadeIn(float duration = 1, Action callback = null)
        {
            foreach (var g in groups) g.Value.FadeIn(1, callback);
        }

        public void FadeOut(float duration = 1, Action callback = null)
        {
            foreach (var g in groups) g.Value.FadeOut(duration, callback);
        }

        public TGroup GetGroup<TGroup>() where TGroup : AudioGroup
        {
            if (!groups.ContainsKey(typeof(TGroup))) return default;
            return groups[typeof(TGroup)] as TGroup;
        }

        public void Pause()
        {
            foreach (var g in groups) g.Value.Pause();
        }

        public void Resume()
        {
            foreach (var g in groups) g.Value.Resume();
        }

        public void Stop()
        {
            foreach (var g in groups) g.Value.Stop();
        }
    }
}
