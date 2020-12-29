namespace Origine.BT
{
    /// <summary>
    /// 把失败包装成功返回
    /// </summary>
    [DecorateNode("Success")]
    public class SuccessProxy : BaseNodeProxy
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

            if (childNodeStatus == NodeStatus.Failed)
                m_CompositeNode.Status = NodeStatus.Succeed;
        }
    }
}
