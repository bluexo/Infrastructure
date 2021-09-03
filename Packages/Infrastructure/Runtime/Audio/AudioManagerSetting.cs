namespace Origine
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Configs/AudioManagerSetting")]
    public class AudioManagerSetting : ScriptableObject
    {
        public string PreloadAudioPath;
        public int AudioPlayerCount;
        public float BaseVolume = 1f;
        public bool ShouldAdjustVolumeRate;
        public bool IsDestroyManager;
        public bool IsReleaseCache;
        public bool IsAutoUpdateSetting;
        public bool ForceToMono;
        public bool Normalize;
        public bool Ambisonic;
        public bool LoadInBackground;
        public AudioCacheType CacheType;
        public AudioClipLoadType LoadType;
        public AudioCompressionFormat CompressionFormat;
        public float Quality;
    }
}