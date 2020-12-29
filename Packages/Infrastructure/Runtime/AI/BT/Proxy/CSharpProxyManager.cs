
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Origine.BT
{
    public sealed class CSharpProxyManager : IProxyManager
    {
        /// <summary>
        /// 单例
        /// </summary>
        private Dictionary<string, ProxyData> proxies = new Dictionary<string, ProxyData>(StringComparer.OrdinalIgnoreCase);

        public IEnumerator InitializeAsync()
        {
            proxies.Clear();

            var types = Utility.AssemblyCollection
                .GetTypes(t => t.GetCustomAttribute<NodeNameAttribute>(true) != null);

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<NodeNameAttribute>();
                Register(attr.Name, attr.NodeType, type);
            }

            yield return null;
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