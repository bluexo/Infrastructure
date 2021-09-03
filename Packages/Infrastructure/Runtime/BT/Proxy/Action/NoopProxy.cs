
namespace Origine
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