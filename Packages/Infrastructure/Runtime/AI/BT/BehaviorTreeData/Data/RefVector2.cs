using System;
using System.Collections.Generic;
using System.Text;

namespace Origine
{
    public class RefVector2 : Binary
    {
        public int X;
        public int Y;

        public override void Read(ref Reader reader)
        {
            reader.Read(ref X).Read(ref Y);
        }

        public override void Write(ref Writer writer)
        {
            writer.Write(X).Write(Y);
        }

        public RefVector2 Clone()
        {
            RefVector2 vector2 = new RefVector2();
            vector2.X = X;
            vector2.Y = Y;
            return vector2;
        }
    }
}