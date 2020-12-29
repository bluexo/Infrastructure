using Origine;

namespace Origine.BT
{
    /// <summary>
    ///  等待帧数节点
    /// </summary>
    [ActionNode("WaitFrames")]
    public class WaitFramesProxy : BaseNodeProxy
    {
        private int m_Frames = -1;
        private int m_CurFrames = -1;

        public override void OnAwake()
        {
            IntField framesField = Node.NodeData["Frames"] as IntField;
            if (framesField == null || framesField.Value <= 0)
            {
                Node.Status = NodeStatus.Error;
                return;
            }

            m_Frames = framesField;
        }

        public override void OnStart()
        {
            m_CurFrames = 0;
        }

        public override void OnUpdate(float deltatime)
        {
            m_CurFrames++;

            if (m_CurFrames >= m_Frames)
                Node.Status = NodeStatus.Succeed;
        }
    }
}
