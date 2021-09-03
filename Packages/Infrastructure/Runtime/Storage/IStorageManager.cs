using System;
using System.Collections.Generic;

namespace Origine
{
    /// <summary>
    /// 游戏配置管理器接口。
    /// </summary>
    public interface IStorageManager
    {
        /// <summary>
        /// 获取或者创建数据集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        IStorageItem<T> GetOrCreate<T>(string name = default) where T : new();

        void SaveData<T>(Action<T> action, string name = default) where T : new();

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool Exists(string name);
    }
}
