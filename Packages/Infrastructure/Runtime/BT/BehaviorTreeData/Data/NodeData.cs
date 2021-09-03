using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Origine
{
    public partial class NodeData : Binary
    {
        public int X;
        public int Y;

        public Guid ID;
        public string ClassType;
        public string Label;
        public List<BaseField> Fields = new List<BaseField>();
        public List<NodeData> Childs = null;

        public BaseField this[string fieldName] => Fields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

        public override void Read(ref Reader reader)
        {
            reader.Read(ref X).Read(ref Y).Read(ref ID).Read(ref ClassType).Read(ref Label).Read(ref Fields).Read(ref Childs);
        }

        public override void Write(ref Writer writer)
        {
            writer.Write(X).Write(Y).Write(ID).Write(ClassType).Write(Label).Write(Fields).Write(Childs);
        }
    }
}
