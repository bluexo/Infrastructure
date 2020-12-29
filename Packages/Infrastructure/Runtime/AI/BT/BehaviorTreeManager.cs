
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Origine.BT
{
    public interface IBTManager
    {
        IEnumerator InitializeAsync(string path, IProxyManager proxy = null);
        IReadOnlyList<BehaviorTree> RunningTrees { get; }
        BehaviorTree Create(string treeType, string behaviorTreeId, object context);
        bool TryCreate(string treeType, string behaviorTreeId, object context, out BehaviorTree behaviorTree);
        void Run(BehaviorTree behaviorTree);
        void Recycle(BehaviorTree behaviorTree);
    }

    public class BehaviorTreeManager : GameModule, IBTManager
    {
        public IReadOnlyList<BehaviorTree> RunningTrees => runnings;
        private readonly List<BehaviorTree> runnings = new List<BehaviorTree>();
        private readonly Dictionary<string, BehaviorTreeData> dataCache = new Dictionary<string, BehaviorTreeData>();
        private readonly Dictionary<string, Dictionary<string, Queue<BehaviorTree>>> pools = new Dictionary<string, Dictionary<string, Queue<BehaviorTree>>>();
        private IProxyManager _proxyManager;

        /// <summary>
        /// 加载指定类型的行为树
        /// </summary>
        /// <param name="path">行为树类型</param>
        public IEnumerator InitializeAsync(string path, IProxyManager proxyManager)
        {
            _proxyManager = proxyManager ?? new CSharpProxyManager();
            yield return _proxyManager.InitializeAsync();
            var handle = Addressables.LoadAssetsAsync<TextAsset>(path, t =>
            {
                var treeData = Serializer.Deserialize<BehaviorTreeData>(t.bytes);
                if (treeData != null) dataCache[t.name] = treeData;
            });
            yield return handle;
        }

        /// <summary>
        /// 根据类型获取AgentData
        /// </summary>
        /// <param name="behaviorTreeType">行为树类型</param>
        /// <param name="id">行为树id</param>
        /// <returns></returns>
        public BehaviorTreeElement GetBehaviroTreeData(string behaviorTreeType, string id)
        {
            if (!dataCache.TryGetValue(behaviorTreeType, out BehaviorTreeData treeData))
                throw new Exception($"There is no treeData,BehaviorTreeType:{behaviorTreeType}.");

            if (treeData == null)
                return null;

            for (int i = 0; i < treeData.BehaviorTrees.Count; i++)
            {
                var behaviorTreeElement = treeData.BehaviorTrees[i];
                if (behaviorTreeElement != null && behaviorTreeElement.Id == id)
                    return behaviorTreeElement;
            }

            var msg = $"There is no AgentData,BehaviorTreeType:{behaviorTreeType} id:{id}.";
            throw new Exception(msg);
        }

        /// <summary>
        /// 创建行为树
        /// </summary>
        /// <param name="behaviorTreeType">行为树类型</param>
        /// <param name="behaviorTreeId">行为树ID</param>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        public BehaviorTree Create(string behaviorTreeType, string behaviorTreeId, object context)
        {
            BehaviorTree behaviorTree = Take(behaviorTreeType, behaviorTreeId);

            if (behaviorTree == null)
            {
                var msg = $"BehaviorTreeManager.CreateBehaviorTree() \n Create failed, BehaviorTreeType:{behaviorTreeType} AgentId:{behaviorTreeId}.";
                throw new Exception(msg);
            }

            behaviorTree.SetContext(context);
            behaviorTree.SetProxyCreator(CreateProxy);

            return behaviorTree;
        }

        public bool TryCreate(string treeType,
            string behaviorTreeId,
            object context,
            out BehaviorTree behaviorTree)
        {
            behaviorTree = null;

            try
            {
                behaviorTree = Create(treeType, behaviorTreeId, context);
                return behaviorTree != null;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 激活行为树
        /// </summary>
        /// <param name="behaviorTree"></param>
        public void Run(BehaviorTree behaviorTree)
        {
            if (behaviorTree == null)
                throw new Exception("BehaviorTreeManager.ActiveBehaviorTree() \n Run failed, behaviorTree is null.");

            if (runnings.Contains(behaviorTree))
                throw new Exception("BehaviorTreeManager.ActiveBehaviorTree() \n behaviorTree already run");

            runnings.Add(behaviorTree);
            behaviorTree.Run(0);
        }

        /// <summary>
        /// 从池中产生行为树，如果没有就创建
        /// </summary>
        /// <param name="behaviorTreeType">行为树类型</param>
        /// <param name="behaviorTreeId">行为树ID</param>
        /// <returns>行为树</returns>
        private BehaviorTree Take(string behaviorTreeType, string behaviorTreeId)
        {
            Queue<BehaviorTree> queue = null;

            do
            {
                if (!pools.TryGetValue(behaviorTreeType, out Dictionary<string, Queue<BehaviorTree>> typePoolDic))
                    break;

                if (!typePoolDic.TryGetValue(behaviorTreeId, out queue))
                    break;
            }
            while (false);

            BehaviorTree behaviorTree = null;

            if (queue != null && queue.Count > 0)
            {
                while (behaviorTree == null && queue.Count > 0)
                    behaviorTree = queue.Dequeue();
            }

            if (behaviorTree == null)
            {
                behaviorTree = new BehaviorTree();
                BehaviorTreeElement behaviorTreeElement = GetBehaviroTreeData(behaviorTreeType, behaviorTreeId);
                behaviorTree.SetData(behaviorTreeElement);
                behaviorTree.StartNode = CreateNode(behaviorTreeElement.StartNode);
                behaviorTree.BehaviorTreeType = behaviorTreeType;
            }

            return behaviorTree;
        }

        /// <summary>
        /// 回收到缓存池
        /// </summary>
        /// <param name="behaviorTree">回收的行为树</param>
        public void Recycle(BehaviorTree behaviorTree)
        {
            if (behaviorTree == null)
            {
                string msg = $"BehaviorTreeManager.Despawn() \n behaviorTree is null.";
                throw new Exception(msg);
            }
            behaviorTree.Destroy();

            var btType = behaviorTree.BehaviorTreeType;
            if (!pools.TryGetValue(btType, out Dictionary<string, Queue<BehaviorTree>> btPool))
            {
                btPool = new Dictionary<string, Queue<BehaviorTree>>();
                pools.Add(btType, btPool);
            }

            var btId = behaviorTree.BehaviorTreeId;
            if (!btPool.TryGetValue(btId, out Queue<BehaviorTree> queue))
            {
                queue = new Queue<BehaviorTree>();
                btPool.Add(btId, queue);
            }

            if (queue.Contains(behaviorTree))
            {
                string msg = $"BehaviorTreeManager.Despawn() \n queue contains behaviorTree,behaviorTreeId:{btId}.";
                throw new Exception(msg);
            }

            queue.Enqueue(behaviorTree);
        }

        /// <summary>
        /// 清除所有缓存数据
        /// </summary>
        public void ClearPool() => pools.Clear();

        /// <summary>
        /// 清除指定类型的缓存数据
        /// </summary>
        /// <param name="behaviorTreeType">行为树类型</param>
        public void ClearPool(string behaviorTreeType)
        {
            if (!pools.TryGetValue(behaviorTreeType, out Dictionary<string, Queue<BehaviorTree>> typePool))
                return;

            if (typePool != null)
                typePool.Clear();
        }

        /// <summary>
        /// 构建节点
        /// </summary>
        /// <param name="nodeData">节点数据</param>
        public BaseNode CreateNode(NodeData nodeData)
        {
            if (nodeData == null)
                throw new Exception($"BehaviorTreeManager.CreateNode() \n nodeData is null.");

            var proxyData = _proxyManager.GetProxyData(nodeData.ClassType);

            if (proxyData == null)
                throw new Exception($"BehaviorTreeManager.CreateNode({nodeData.ClassType}) \n proxyData is null.");

            BaseNode baseNode = null;

            switch (proxyData.NodeType)
            {
                case NodeType.Action:
                    baseNode = new ActionNode { NodeType = NodeType.Action };
                    break;
                case NodeType.Composite:
                    baseNode = new CompositeNode { NodeType = NodeType.Composite };
                    break;
                case NodeType.Condition:
                    baseNode = new ConditionNode { NodeType = NodeType.Condition };
                    break;
                case NodeType.Decorator:
                    baseNode = new DecorateNode { NodeType = NodeType.Condition };
                    break;
            }

            if (baseNode == null)
                throw new Exception($"CreateNode {proxyData.Name} Failed");

            baseNode.SetData(nodeData);
            baseNode.SetProxyData(proxyData);

            if (baseNode is CompositeNode)
            {
                if (nodeData.Childs == null || nodeData.Childs.Count == 0)
                    throw new Exception($"{proxyData.NodeType} node must have child nodes");

                var compositeNode = baseNode as CompositeNode;
                for (int i = 0; i < nodeData.Childs.Count; i++)
                {
                    var childNodeData = nodeData.Childs[i];
                    var childNode = CreateNode(childNodeData);
                    compositeNode.AddChild(childNode);
                }
            }

            return baseNode;
        }

        public BaseNodeProxy CreateProxy(BaseNode node)
        {
            if (node == null)
                throw new Exception("BehaviorTreeManager.CreateProxy() \n Create nodeProxy failed,node is null.");

            node.Status = NodeStatus.None;

            if (_proxyManager == null)
                throw new Exception($"BehaviorTreeManager.CreateProxy({node.NodeData.ClassType}) \n Create nodeProxy failed,proxyManager is null.");

            var nodeProxy = _proxyManager.Create(node);
            if (nodeProxy == null)
                throw new Exception($"BehaviorTreeManager.CreateProxy({node.NodeData.ClassType}) \n Create nodeProxy failed,ClassType:{node.ProxyData.Name}");

            return nodeProxy;
        }

        public override void OnUpdate(float deltaTime)
        {
            var resetTreeCount = 0;

            for (int i = 0; i < runnings.Count; i++)
            {
                var behaviorTree = runnings[i];

                if (behaviorTree == null)
                {
                    resetTreeCount++;
                    continue;
                }

                behaviorTree.Run(deltaTime);

                if (behaviorTree.Status == NodeStatus.Succeed
                    || behaviorTree.Status == NodeStatus.Failed
                    || behaviorTree.Status == NodeStatus.Error)
                {
                    runnings[i] = null;
                    behaviorTree.Reset();
                }
            }

            if (resetTreeCount > runnings.Count >> 2)
            {
                runnings.RemoveAll(r => r == null);
            }
        }

        public bool HasTree(string treeType, string treeId)
        {
            if (!dataCache.TryGetValue(treeType, out BehaviorTreeData data)) return false;
            return data.BehaviorTrees.Any(t => t.Id == treeId);
        }
    }
}