using UnityEngine;

namespace Origine
{
    [ActionNode("Log")]
    public class LogProxy : BaseNodeProxy
    {
        public override void OnStart()
        {
            string content = NodeData["Content"];
            Debug.Log(content);
            Node.Status = NodeStatus.Succeed;
        }
    }
}
