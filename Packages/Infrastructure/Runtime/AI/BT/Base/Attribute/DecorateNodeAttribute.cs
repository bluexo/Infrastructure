namespace Origine.BT
{
    public class DecorateNodeAttribute : NodeNameAttribute
    {
        public DecorateNodeAttribute(string classType) : base(classType, NodeType.Decorator)
        {
        }
    }
}
