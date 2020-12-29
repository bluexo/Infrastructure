
using System;
using System.Collections.Generic;

namespace Origine.BT
{
    public class Blackboard
    {
        private readonly Dictionary<string, object> dataSet = new Dictionary<string, object>(System.StringComparer.OrdinalIgnoreCase);
        public void SetData(string name, object value) => dataSet[name] = value;
        public object GetData(string name) => dataSet[name];
        public T GetData<T>(string name) => (T)dataSet[name];

        public bool TryGetData<T>(string name, out T t)
        {
            t = default;
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (!dataSet.ContainsKey(name)) return false;
            if (!(dataSet[name] is T temp)) return false;
            t = temp;
            return true;
        }
        public bool HasData(string name) => dataSet.ContainsKey(name);

        public void DeleteData(string name) => dataSet.Remove(name);
        public void Clear() => dataSet.Clear();
    }

    public class BehaviorTree
    {
        public string BehaviorTreeId { get; private set; }
        public BehaviorTreeElement BehaviorTreeElement { get; private set; }
        public NodeStatus Status { get; private set; } = NodeStatus.None;
        public object Context { get; private set; }

        public BaseNode StartNode { get; set; }
        public string BehaviorTreeType { get; set; }
        public bool IsRunning => Status == NodeStatus.Running;

        public readonly Blackboard Blackboard = new Blackboard();
        public event EventHandler<NodeStatus> OnCompleted;

        public void SetData(BehaviorTreeElement behaviorTreeElement)
        {
            BehaviorTreeElement = behaviorTreeElement;
            BehaviorTreeId = behaviorTreeElement.Id;
        }

        public void SetContext(object context)
        {
            Context = context;
            StartNode?.SetContext(this, context);
        }

        public void SetProxyCreator(Func<BaseNode, BaseNodeProxy> creator) => StartNode?.SetProxyCreator(creator);

        public void Run(float deltatime)
        {
            if (StartNode == null)
                return;

            StartNode.Run(deltatime);

            Status = StartNode.Status;
        }

        public void SetActive(bool active) => StartNode?.SetActive(active);

        public void Reset()
        {
            OnCompleted?.Invoke(this, Status);
            Status = NodeStatus.None;
            StartNode?.Reset();
            Blackboard.Clear();
        }

        internal void Destroy() => StartNode?.Destroy();
    }
}