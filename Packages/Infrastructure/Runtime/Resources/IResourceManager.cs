using System.Collections.Generic;

using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Origine
{
    public interface IResourceManager
    {
        Dictionary<string, Object> LoadedAssets { get; }

        AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(string key) where TObject : Object;

        void ReleaseAsset<TObject>(string key);
    }
}
