using System;
using System.IO;

using System.Collections.Generic;

using UnityEngine;
using UltraLiteDB;

namespace Origine
{
    public interface IStorageItem<T>
    {
        bool IsNullOrEmpty { get; }
        T Value { get; set; }

        void Save();
        void Clear();
    }

    /// <summary>
    /// 数据存储管理器。
    /// </summary>
    internal sealed class StorageManager : GameModule, IStorageManager
    {
        public const string DatabaseName = "GameData.db";

        private readonly UltraLiteDatabase _database;
        private readonly Dictionary<string, object> _items = new Dictionary<string, object>();

        public StorageManager()
        {
            var filepath = Application.isEditor ? Application.streamingAssetsPath : Application.persistentDataPath;
            if (!Directory.Exists(filepath)) Directory.CreateDirectory(filepath);
            _database = new UltraLiteDatabase(new ConnectionString($"Filename={filepath}/{DatabaseName};Password=efcf8794-8c30-4d1b-940a-df1475c7b26d"));
        }

        public bool Exists(string name) => _database.CollectionExists(name);

        public IStorageItem<T> GetOrCreate<T>(string name = default) where T : new()
        {
            if (string.IsNullOrWhiteSpace(name))
                name = typeof(T).Name;
            if (!_items.ContainsKey(name))
            {
                var item = new LiteDBStorageItem<T>(_database, name);
                _items[name] = item;
            }
            return (IStorageItem<T>)_items[name];
        }

        public override void OnDispose()
        {
            base.OnDispose();
            _database.Dispose();
        }

        public void SaveData<T>(Action<T> action, string name = default) where T : new()
        {
            var storageItem = GetOrCreate<T>(name);
            action?.Invoke(storageItem.Value);
            storageItem.Save();
        }
    }

    public static class StorageUtility
    {

#if UNITY_EDITOR


        [UnityEditor.MenuItem("Tools/Storage/Clear")]
        public static void ClearGameData()
        {
            var path = $"Assets/StreamingAssets/{StorageManager.DatabaseName}";
            UnityEditor.AssetDatabase.DeleteAsset(path);
            Debug.Log($"Clear db file :{path}");
        }
#endif
    }
}
