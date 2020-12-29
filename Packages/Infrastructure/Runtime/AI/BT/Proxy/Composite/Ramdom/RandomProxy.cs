using System;

namespace Origine.BT
{
    /// <summary>
    /// 随机选择一个节点，并将随机节点的结果返回
    /// </summary>
    [CompositeNode("Random")]
    public class RandomProxy : BaseNodeProxy
    {
        private CompositeNode m_CompositeNode;
        private Random m_Random = new Random();

        public override void OnStart()
        {
            m_CompositeNode = Node as CompositeNode;
            m_CompositeNode.RunningNodeIndex = m_Random.Next(0, m_CompositeNode.Childs.Count);
        }

        public override void OnUpdate(float deltatime)
        {
            BaseNode childNode = m_CompositeNode.Childs[m_CompositeNode.RunningNodeIndex];
            childNode.Run(deltatime);
            NodeStatus childNodeStatus = childNode.Status;

            if (childNodeStatus == NodeStatus.Error)
            {
                m_CompositeNode.Status = NodeStatus.Error;
                return;
            }

            if (childNodeStatus > NodeStatus.Running)
                m_CompositeNode.Status = childNodeStatus;
        }
    }
}