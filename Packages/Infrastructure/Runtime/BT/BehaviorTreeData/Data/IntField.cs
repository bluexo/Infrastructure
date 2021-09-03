﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Origine
{
    public partial class IntField : BaseField
    {
        public int Value;

        public override void Read(ref Reader reader) => reader.Read(ref FieldName).Read(ref Value);
        public override void Write(ref Writer writer) => writer.Write(FieldName).Write(Value);

        #region operator

        public static implicit operator int(IntField field) => field.Value;
        public static explicit operator IntField(int value) => new IntField { Value = value };

        #endregion
    }
}
