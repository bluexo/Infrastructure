using System.Collections.Generic;

namespace Origine
{
    /// <summary>
    /// 游戏配置管理器接口。
    /// </summary>
    public interface ISettingManager
    {
        /// <summary>
        /// 获取游戏配置项数量。
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取所有游戏配置项的名称。
        /// </summary>
        /// <returns>所有游戏配置项的名称。</returns>
        IEnumerable<string> GetAllSettingNames();

        /// <summary>
        /// 检查是否存在指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要检查游戏配置项的名称。</param>
        /// <returns>指定的游戏配置项是否存在。</returns>
        bool HasSetting(string settingName);

        /// <summary>
        /// 移除指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要移除游戏配置项的名称。</param>
        /// <returns>是否移除指定游戏配置项成功。</returns>
        bool RemoveSetting(string settingName);

        /// <summary>
        /// 清空所有游戏配置项。
        /// </summary>
        void RemoveAllSettings();

        T Get<T>(string name, T t = default);

        bool TryGet<T>(string name, out T t);

        void Set<T>(string name, T t);
    }
}
