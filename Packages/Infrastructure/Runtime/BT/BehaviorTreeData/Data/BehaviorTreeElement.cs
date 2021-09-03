using System;
using System.Collections.Generic;
using System.Text;

namespace Origine
{
    public partial class BehaviorTreeElement : Binary
    {
        public string Id;
        public List<BaseField> Fields = new List<BaseField>();
        public List<BaseField> BehaviorTreeVariables = new List<BaseField>();
        public NodeData StartNode;

        public override void Read(ref Reader reader)
        {
            reader.Read(ref Id).Read(ref Fields).Read(ref BehaviorTreeVariables).Read(ref StartNode);
        }

        public override void Write(ref Writer writer)
        {
            writer.Write(Id).Write(Fields).Write(BehaviorTreeVariables).Write(StartNode);
        }
    }
}
