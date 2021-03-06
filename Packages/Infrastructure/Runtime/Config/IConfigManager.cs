﻿using System.Collections;
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

        bool TryGetData<TData>(int id, out TData data);

        TConfig Get<TConfig>() where TConfig : class, IEnumerable;
    }
}