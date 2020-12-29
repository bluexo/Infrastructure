using Origine;

namespace Origine.BT
{
    /// <summary>
    /// 时间节点
    /// 在指定的时间内，持续调用其子节点
    /// </summary>
    [DecorateNode("Time")]
    public class TimeProxy : BaseNodeProxy
    {
        private CompositeNode m_CompositeNode;
        int Duration = -1;
        float CurTime = -1;

        public override void OnAwake()
        {
            IntField durationField = Node.NodeData["Duration"] as IntField;
            if (durationField == null || durationField.Value <= 0)
            {
                Node.Status = NodeStatus.Error;
                return;
            }

            Duration = durationField;
        }

        public override void OnStart()
        {
            m_CompositeNode = Node as CompositeNode;
            CurTime = 0;
        }

        public override void OnUpdate(float deltatime)
        {
            CurTime += deltatime;
            BaseNode childNode = m_CompositeNode.Childs[0];
            childNode.Run(deltatime);
            NodeStatus childNodeStatus = childNode.Status;

            if (childNodeStatus == NodeStatus.Error)
            {
                m_CompositeNode.Status = NodeStatus.Error;
                return;
            }

            if (CurTime >= Duration / 1000f)
            {
                m_CompositeNode.Status = NodeStatus.Succeed;
                return;
            }

            if (childNodeStatus == NodeStatus.Failed || childNodeStatus == NodeStatus.Succeed)
            {
                childNode.Reset();
            }
        }
    }
}