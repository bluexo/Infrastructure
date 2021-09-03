using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Love.Network
{
    public static class ObjectMapper
    {
        public readonly static Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();

        static ObjectMapper()
        {
            TypeCache = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(NetworkObject)))
                .ToDictionary(k => k.Name, v => v);

            foreach (var type in TypeCache.Values)
                Log.Info(nameof(ObjectMapper), $"{type.FullName}");
        }

        public static Type Get(string name)
        {
            if (!TypeCache.TryGetValue(name, out Type type))
                throw new NullReferenceException($"Cannot found NetworkObject , name = {name}");
            return type;
        }

        public static T CreateInstance<T>(string name, params object[] parameters) where T : NetworkObject
        {
            var type = Get(name);

            return (T)Activator.CreateInstance(type, parameters);
        }
    }
}