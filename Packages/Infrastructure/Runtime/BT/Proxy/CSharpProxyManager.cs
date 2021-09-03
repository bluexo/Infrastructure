
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Origine
{
    public class CSharpProxyManager : IProxyManager
    {
        /// <summary>
        /// 单例
        /// </summary>
        private Dictionary<string, ProxyData> proxies = new Dictionary<string, ProxyData>(StringComparer.OrdinalIgnoreCase);

        private readonly List<(Type type, NodeNameAttribute)> typeCache = new List<(Type type, NodeNameAttribute)>();

        public CSharpProxyManager()
        {
            var types = AssemblyCollection.GetTypes(t => t.IsSubclassOf(typeof(BaseNodeProxy)) && !t.IsAbstract);
            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<NodeNameAttribute>();
                if (attr == null) continue;
                typeCache.Add((type, attr));
            }
        }


        public void Initialize()
        {
            proxies.Clear();

            foreach (var (type, attr) in typeCache)
            {
                Register(attr.Name, attr.NodeType, type);
            }
        }

        public ProxyData GetProxyData(string classType)
        {
            proxies.TryGetValue(classType, out ProxyData proxyData);
            return proxyData;
        }

        public BaseNodeProxy Create(BaseNode Node)
        {
            ProxyData proxyData = Node.ProxyData;
            Type type = GetType(proxyData.Name);
            var proxy = Activator.CreateInstance(type) as BaseNodeProxy;
            proxy.BeginInit();
            proxy.SetNode(Node);
            proxy.EndInit();
            return proxy;
        }

        /// <summary>
        /// 注册Proxy
        /// </summary>
        /// <param name="classType">节点类</param>
        /// <param name="nodeType">节点类型</param>
        /// <param name="type">逻辑对应的Type</param>
        public void Register(string classType, NodeType nodeType, Type type)
        {
            if (string.IsNullOrEmpty(classType))
                throw new Exception("CSharpProxyManager.Register() \n classType is null.");

            if (proxies.ContainsKey(classType))
                throw new Exception($"CSharpProxyManager.Register() \n m_ProxyDic already Contain key {classType}.");

            var proxyData = new ProxyData
            {
                Name = classType,
                NodeType = nodeType,
                Type = type,
                NeedUpdate = false
            };

            proxies.Add(classType, proxyData);
        }

        private Type GetType(string classType)
        {
            if (!proxies.ContainsKey(classType))
                throw new Exception($"CSharpProxyManager.GetType() \n m_ProxyTypeDic not contains key {classType}.");
            var type = proxies[classType].Type;
            return type;
        }
    }
}