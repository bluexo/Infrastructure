namespace Origine.BT
{
    /// <summary>
    /// 直到子节点返回成功
    /// 子节点返回失败直接重置继续执行
    /// </summary>
    [DecorateNode("SuccessUntil")]
    public class SuccessUntilProxy : BaseNodeProxy
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

            if (childNodeStatus == NodeStatus.Succeed)
            {
                m_CompositeNode.Status = NodeStatus.Succeed;
                return;
            }

            if (childNodeStatus == NodeStatus.Failed)
            {
                childNode.Reset();
            }
        }
    }
}