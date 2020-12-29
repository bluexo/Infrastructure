namespace Origine.BT
{
    public class CompositeNodeAttribute : NodeNameAttribute
    {
        public CompositeNodeAttribute(string classType) : base(classType, NodeType.Composite)
        {
        }
    }
}