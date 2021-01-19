using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Origine
{
    /// <summary>
    /// 程序集相关的实用函数。
    /// </summary>
    public static class AssemblyCollection
    {
        private static readonly Assembly[] _assemblies = null;
        private static readonly Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();
        private static readonly HashSet<Type> _allTypes = new HashSet<Type>();

        static AssemblyCollection()
        {
            _assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in _assemblies)
                _allTypes.UnionWith(GetLoadableTypes(asm));

        }

        /// <summary>
        /// 获取已加载的程序集。
        /// </summary>
        /// <returns>已加载的程序集。</returns>
        public static Assembly[] GetAssemblies() => _assemblies;

        /// <summary>
        /// 获取已加载的程序集中的所有类型。
        /// </summary>
        /// <returns>已加载的程序集中的所有类型。</returns>
        public static Type[] GetTypes(Func<Type, bool> predicate = null)
        {
            var results = predicate == null
                ? _allTypes
                : _allTypes.Where(predicate);
            return results.ToArray();
        }

        /// <summary>
        /// 获取已加载的程序集中的所有类型。
        /// </summary>
        /// <param name="results">已加载的程序集中的所有类型。</param>
        public static void GetTypes(List<Type> results)
        {
            if (results == null)
            {
                throw new GameException("Results is invalid.");
            }

            results.Clear();

            results.AddRange(_allTypes);
        }

        public static IEnumerable<Type> GetTypesWithAttribute<T>() where T : Attribute
        {
            foreach (Type type in _allTypes)
            {
                if (type.GetCustomAttributes(typeof(T), true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        public static Type FilterTypeWithAttribute<T>(Predicate<T> predicate) where T : Attribute
        {
            foreach (Type type in _allTypes)
            {
                var attributes = type.GetCustomAttributes(typeof(T), true);
                if (attributes.Length <= 0) continue;
                var first = (T)attributes.FirstOrDefault();
                if (first == null) continue;
                if (predicate(first)) return type;
            }

            return default;
        }

        public static bool TryGetAttribute<T>(this MemberInfo memberInfo, out T attr) where T : Attribute
        {
            attr = default;
            var attribute = memberInfo.GetCustomAttributes(typeof(T), true);
            if (attribute.Length <= 0)
                return false;
            attr = (T)attribute.FirstOrDefault();
            return true;
        }

        public static IEnumerable<(Type, object[])> GetTypeAttributesData<T>() where T : Attribute
        {
            foreach (Type type in _allTypes)
            {
                var attributes = type.GetCustomAttributes(typeof(T), true);
                if (attributes.Length > 0)
                {
                    yield return (type, attributes);
                }
            }
        }

        public static Type GetType(Func<Type, bool> predicate) => _allTypes.FirstOrDefault(predicate);

        public static Type GetType(string typeName) => GetType(typeName, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 获取已加载的程序集中的指定类型。
        /// </summary>
        /// <param name="typeName">要获取的类型名。</param>
        /// <returns>已加载的程序集中的指定类型。</returns>
        public static Type GetType(string typeName, StringComparison comparison)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new GameException("Type name is invalid.");
            }

            if (_cachedTypes.TryGetValue(typeName, out Type type))
            {
                return type;
            }

            type = Type.GetType(typeName) ?? GetType(t => t.Name.Equals(typeName, comparison));

            if (type != null)
            {
                _cachedTypes[typeName] = type;
                return type;
            }

            foreach (Assembly assembly in _assemblies)
            {
                type = Type.GetType(string.Format("{0}, {1}", typeName, assembly.FullName));
                if (type != null)
                {
                    _cachedTypes.Add(typeName, type);
                    return type;
                }
            }

            return null;
        }

        public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
