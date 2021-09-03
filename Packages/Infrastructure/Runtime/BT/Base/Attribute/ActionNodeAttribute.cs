
namespace Origine
{
    public class ActionNodeAttribute : NodeNameAttribute
    {
        public ActionNodeAttribute(string classType) : base(classType, NodeType.Action)
        {
        }
    }
}