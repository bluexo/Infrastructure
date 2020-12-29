namespace Origine.BT
{
    /// <summary>
    /// 将子节点结果以失败返回
    /// </summary>
    [DecorateNode("Failure")]
    public class FailureProxy : BaseNodeProxy
    {
        private CompositeNode m_CompositeNode;

        public override void OnStart()
        {
            m_CompositeNode = Node as CompositeNode;
        }

        public override void OnUpdate(float deltatime)
        {
            BaseNode childNode = m_CompositeNode.Childs[0];
            childNode.Run(deltatime);
            NodeStatus childNodeStatus = childNode.Status;

            if (childNodeStatus == NodeStatus.Error)
            {
                m_CompositeNode.Status = NodeStatus.Error;
                return;
            }

            if (childNodeStatus > NodeStatus.Running)
                m_CompositeNode.Status = NodeStatus.Failed;
        }
    }
}