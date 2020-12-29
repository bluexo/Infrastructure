using System;

namespace Origine.BT
{
    public class ProxyData
    {
        /// <summary>
        /// 对应的节点类
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 节点类型
        /// </summary>
        public NodeType NodeType { get; set; }

        /// <summary>
        /// 是否需要执行OnUpdate（lua需要Update才开启）
        /// </summary>
        public bool NeedUpdate { get; set; }
    }
}