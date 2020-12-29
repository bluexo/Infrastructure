using System;

namespace Origine.BT
{
    public abstract class NodeNameAttribute : Attribute
    {
        public NodeNameAttribute(string classType, NodeType nodeType)
        {
            Name = classType;
            NodeType = nodeType;
        }

        public string Name { get; set; }
        public NodeType NodeType { get; set; } = NodeType.Action;
    }
}