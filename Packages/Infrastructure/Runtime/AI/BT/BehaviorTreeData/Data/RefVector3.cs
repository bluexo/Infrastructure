using System;
using System.Collections.Generic;
using System.Text;

namespace Origine
{
    public class RefVector3 : Binary
    {
        public int X;
        public int Y;
        public int Z;

        public override void Read(ref Reader reader)
        {
            reader.Read(ref X).Read(ref Y).Read(ref Z);
        }

        public override void Write(ref Writer writer)
        {
            writer.Write(X).Write(Y).Write(Z);
        }

        public RefVector3 Clone()
        {
            RefVector3 vector3 = new RefVector3();
            vector3.X = X;
            vector3.Y = Y;
            vector3.Z = Z;
            return vector3;
        }
    }
}