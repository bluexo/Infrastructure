namespace Origine.BT
{
    [CompositeNode("Sequence")]
    public class SequenceProxy : BaseNodeProxy
    {
        private CompositeNode m_CompositeNode;

        public override void OnStart()
        {
            m_CompositeNode = Node as CompositeNode;
        }

        public override void OnUpdate(float deltatime)
        {
            for (int i = m_CompositeNode.RunningNodeIndex; i < m_CompositeNode.Childs.Count;)
            {
                BaseNode childNode = m_CompositeNode.Childs[i];
                childNode.Run(deltatime);
                NodeStatus childNodeStatus = childNode.Status;

                if (childNodeStatus == NodeStatus.Running || childNodeStatus == NodeStatus.Failed || childNodeStatus == NodeStatus.Error)
                {
                    m_CompositeNode.Status = childNodeStatus;
                    return;
                }

                if (childNodeStatus == NodeStatus.Succeed)
                {
                    i++;
                    m_CompositeNode.RunningNodeIndex++;

                    if (i == m_CompositeNode.Childs.Count)
                        m_CompositeNode.Status = NodeStatus.Succeed;
                }
            }
        }
    }
}
