using System.Collections;
using System.Collections.Generic;

namespace Origine
{
    /// <summary>
    /// 配置管理器
    /// </summary>
    public interface IConfigManager
    {
        IEnumerator LoadConfigFilesAsync(string path);

        TData GetData<TData>(int id);

        TConfig Get<TConfig>() where TConfig : class, IEnumerable;
    }
}