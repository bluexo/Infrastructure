using UnityEngine;

namespace Origine.BT
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
