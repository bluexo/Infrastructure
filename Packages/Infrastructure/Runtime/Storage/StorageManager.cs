﻿
using System.IO;
using SQLite4Unity3d;

using System.Collections.Generic;

using UnityEngine;

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
        private const string DatabaseName = "GameData.db";

        private readonly SQLiteConnection _connection;
        private readonly Dictionary<string, object> _items = new Dictionary<string, object>();

        public StorageManager()
        {
            var filepath = Application.isEditor ? Application.streamingAssetsPath : Application.persistentDataPath;
            if (!Directory.Exists(filepath)) Directory.CreateDirectory(filepath);
            _connection = new SQLiteConnection($"{filepath}/{DatabaseName}", SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        }

        public bool Exists(string name) => _connection.GetTableInfo(name) != null;

        public IStorageItem<T> GetOrCreate<T>(string name) where T : new()
        {
            if (!_items.ContainsKey(name))
            {
                var item = new SqliteStorageItem<T>(_connection, name);
                _items[name] = item;
            }
            return (IStorageItem<T>)_items[name];
        }

        public override void OnDispose()
        {
            base.OnDispose();
            _connection.Dispose();
        }
    }
}
