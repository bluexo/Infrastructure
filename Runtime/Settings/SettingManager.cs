using System;
using System.Collections.Generic;
using System.Linq;

namespace Origine.Setting
{
    /// <summary>
    /// 游戏配置管理器。
    /// </summary>
    internal sealed class SettingManager : GameModule, ISettingManager
    {
        public class SettingData
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public object Data { get; set; }
        }

        private readonly GameContext _gameContext;
        private readonly IStorageItem<Dictionary<string, SettingData>> _collection;
        private readonly IStorageManager _storageManager;
        private const string SETTINGS = "Settings";

        /// <summary>
        /// 初始化游戏配置管理器的新实例。
        /// </summary>
        public SettingManager(GameContext gameContext)
        {
            _gameContext = gameContext;
            _storageManager = gameContext.GetModule<IStorageManager>();
            _collection = _storageManager.GetOrCreate<Dictionary<string, SettingData>>(SETTINGS);
            _collection.Data = new Dictionary<string, SettingData>();
        }

        /// <summary>
        /// 获取游戏配置项数量。
        /// </summary>
        public int Count => _collection.Data.Count();

        /// <summary>
        /// 获取所有游戏配置项的名称。
        /// </summary>
        /// <returns>所有游戏配置项的名称。</returns>
        public IEnumerable<string> GetAllSettingNames() => _collection.Data.Keys;

        /// <summary>
        /// 检查是否存在指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要检查游戏配置项的名称。</param>
        /// <returns>指定的游戏配置项是否存在。</returns>
        public bool HasSetting(string settingName)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameException("Setting name is invalid.");
            }

            if (_collection.Data == null) return false;

            return _collection.Data.ContainsKey(settingName);
        }

        /// <summary>
        /// 移除指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要移除游戏配置项的名称。</param>
        /// <returns>是否移除指定游戏配置项成功。</returns>
        public bool RemoveSetting(string settingName)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameException("Setting name is invalid.");
            }

            if (_collection.Data == null || !_collection.Data.ContainsKey(settingName)) return false;
            _collection.Data.Remove(settingName);
            _collection.Save();
            return true;
        }

        /// <summary>
        /// 清空所有游戏配置项。
        /// </summary>
        public void RemoveAllSettings()
        {
            _collection.Clear();
        }

        public T Get<T>(string name, T t = default)
        {
            if (!HasSetting(name))
            {
                _collection.Data.Add(name, new SettingData { Name = name, Type = typeof(T), Data = t });
                return t;
            }
            else
            {
                if (!_collection.Data.ContainsKey(name)) return default;
                return (T)_collection.Data[name].Data;
            }
        }

        public bool TryGet<T>(string name, out T t)
        {
            t = default;
            if (!HasSetting(name))
            {
                t = default;
                return false;
            }
            if (_collection.Data.ContainsKey(name)) return false;
            var doc = _collection.Data[name];
            if (doc.Data.GetType() != typeof(T))
            {
                t = default;
                return false;
            }
            t = (T)doc.Data;
            return true;
        }

        public void Set<T>(string name, T t)
        {
            _collection.Data[name] = new SettingData { Name = name, Type = typeof(T), Data = t };
            _collection.Save();
        }
    }
}
