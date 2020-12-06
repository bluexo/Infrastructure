using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Origine.Resource
{
    internal sealed class ResourceManager : GameModule, IResourceManager
    {
        public Dictionary<string, UnityEngine.Object> LoadedAssets { get; private set; } = new Dictionary<string, UnityEngine.Object>();
        private readonly GameContext _gameContext;

        public ResourceManager(GameContext gameContext)
        {
            _gameContext = gameContext;
        }

        public AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(string key) where TObject : UnityEngine.Object
        {
            var handle = Addressables.LoadAssetAsync<TObject>(key);
            return handle;
        }

        public void ReleaseAsset<TObject>(string key)
        {
            Addressables.Release(key);
        }

        public override void Dispose()
        {
            foreach (var asset in LoadedAssets) Addressables.Release(asset.Value);
        }
    }
}
