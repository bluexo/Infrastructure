
namespace Origine.BT
{
    [ActionNode("Noop")]
    public class NoopProxy : BaseNodeProxy
    {
        public override void OnStart()
        {
            Node.Status = NodeStatus.Succeed;
        }
    }
}