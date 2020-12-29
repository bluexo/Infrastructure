
using System.ComponentModel;

namespace Origine.BT
{
    public abstract class BaseNodeProxy : GameEntity, ISupportInitialize
    {
        public NodeData NodeData { get; private set; }
        public object Context { get; private set; }
        public BaseNode Node { get; private set; }
        public Blackboard Blackboard => Node.Tree.Blackboard;

        public virtual void BeginInit() { }
        public virtual void EndInit() { }

        protected virtual void SetData(NodeData data) => NodeData = data;
        protected virtual void SetContext(object context) => Context = context;

        public void SetNode(BaseNode node)
        {
            Node = node;
            SetData(node.NodeData);
            SetContext(node.Context);
        }

        public virtual void OnAwake() { }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        public virtual void OnStart() { }

        public virtual void OnReset() { }
    }
}