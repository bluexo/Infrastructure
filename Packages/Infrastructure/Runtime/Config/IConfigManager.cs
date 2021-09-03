using System;
using System.Collections;
using System.Collections.Generic;

namespace Origine
{
    /// <summary>
    /// 配置管理器
    /// </summary>
    public interface IConfigManager
    {
        IEnumerator InitializeAsync(string path);

        IReadOnlyList<Type> ConfigTypes { get; }

        TData GetData<TData>(string id);

        bool TryGetData<TData>(string id, out TData data);

        TData GetData<TData>(int id);

        bool TryGetData<TData>(int id, out TData data);

        TConfig Get<TConfig>() where TConfig : class, IEnumerable;

        object Get(Type configType);
    }
}