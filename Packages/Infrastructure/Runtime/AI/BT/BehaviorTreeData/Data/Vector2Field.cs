using System;
using System.Collections.Generic;
using System.Text;

namespace Origine
{
    public class Vector2Field : BaseField
    {
        public float X;
        public float Y;

        public override void Read(ref Reader reader)
        {
            reader.Read(ref FieldName).Read(ref X).Read(ref Y);
        }

        public override void Write(ref Writer writer)
        {
            writer.Write(FieldName).Write(X).Write(Y);
        }
    }
}
