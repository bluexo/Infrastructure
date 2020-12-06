using System;

using Newtonsoft.Json;

using UnityEngine;

namespace Origine
{
    public interface IStorageItem<T>
    {
        bool IsNullOrEmpty { get; }
        T Value { get; set; }

        string ToJson();
        void FromJson(string json);

        void Save();
        void Clear();
    }

    public class LocalStorageItem<T> : IStorageItem<T>
    {
        public string Key { get; private set; }
        public bool IsNullOrEmpty => !PlayerPrefs.HasKey(Key) || string.IsNullOrWhiteSpace(PlayerPrefs.GetString(Key));

        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new WritablePropertiesOnlyResolver()
        };

        public LocalStorageItem(string key)
        {
            Key = key;
        }

        private T data;
        public T Value
        {
            get => data;
            set
            {
                data = value;
                Save();
            }
        }

        public string ToJson() => JsonConvert.SerializeObject(Value, SerializerSettings);

        public void FromJson(string json) => Value = JsonConvert.DeserializeObject<T>(json, SerializerSettings);

        public void Save()
        {
            var orignal = PlayerPrefs.GetString(Key, string.Empty);
            var json = JsonConvert.SerializeObject(Value, SerializerSettings);
            if (orignal.Equals(json, StringComparison.Ordinal))
                return;

            PlayerPrefs.SetString(Key, json);
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
            Value = default;
        }
    }

    /// <summary>
    /// 数据存储管理器。
    /// </summary>
    internal sealed class StorageManager : GameModule, IStorageManager
    {
        public StorageManager()
        {
        }

        public bool Exists(string name) => PlayerPrefs.HasKey(name);

        public IStorageItem<T> GetOrCreate<T>(string name) where T : new()
        {
            var item = new LocalStorageItem<T>(name);
            if (!PlayerPrefs.HasKey(name)) return item;

            var json = PlayerPrefs.GetString(name);
            item.FromJson(json);
            return item;
        }
    }
}
