using Origine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.AddressableAssets;

using UniRx;
using System.Threading.Tasks;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Origine
{
    public class ConfigManager : GameModule, IConfigManager
    {
        private readonly Dictionary<Type, object> configs = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Dictionary<int, object>> configDatas = new Dictionary<Type, Dictionary<int, object>>();

        public IEnumerator LoadConfigFilesAsync(string path)
        {
            var configTypes = AssemblyCollection
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ScriptableObject)) && t.GetProperty("List") != null)
                .Select(t => (type: t, Key: $"{path}/{t.Name}.asset"))
                .GroupBy(g => g.Key, v => v)
                .ToDictionary(k => k.Key, v => v.FirstOrDefault().type);

            var tasks = new List<Task>();
            foreach (var pair in configTypes)
                tasks.Add(LoadAssetAsync(pair.Key, pair.Value));
            var completeTask = Task.WhenAll(tasks);
            yield return new WaitUntil(() => completeTask.IsCompleted || completeTask.IsFaulted);
        }

        private async Task LoadAssetAsync(string key, Type value)
        {
            var handle = await Addressables.LoadAssetAsync<ScriptableObject>(key).Task;
            if (handle == null) return;
            configs.Add(value, handle);
        }

        public bool TryGetData<TData>(int id, out TData data)
        {
            data = default;
            try
            {
                data = GetData<TData>(id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public TData GetData<TData>(int id)
        {
            if (!configDatas.ContainsKey(typeof(TData)))
            {
                var idProperty = typeof(TData)
                    .GetProperty("Id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (idProperty == null)
                    throw new InvalidOperationException($"Cannot found property {typeof(TData)}.Id");

                var conf = configs.FirstOrDefault(c => typeof(IEnumerable<TData>).IsAssignableFrom(c.Key)).Value;
                configDatas[typeof(TData)] = ((IEnumerable<TData>)conf)
                    .GroupBy(k => (int)idProperty.GetValue(k), v => v)
                    .ToDictionary(k => k.Key, v => (object)v.FirstOrDefault());
            }

            if (!configDatas[typeof(TData)].ContainsKey(id))
                throw new NullReferenceException($"Cannot found config {typeof(TData)}.Id={id}!");

            return (TData)configDatas[typeof(TData)][id];
        }

        public T Get<T>() where T : class, IEnumerable
        {
            var type = typeof(T);

            if (!configs.ContainsKey(type))
            {
                Debug.LogError($"Cannot found config {type.FullName}");
                return default;
            }

            return (T)configs[type];
        }
    }
}
