using System;
using System.Collections.Generic;
using System.Text;

namespace Origine
{
    public partial class StringField : BaseField
    {
        public string Value;

        public override void Read(ref Reader reader) => reader.Read(ref FieldName).Read(ref Value);
        public override void Write(ref Writer writer) => writer.Write(FieldName).Write(Value);

        public static implicit operator string(StringField field) => field.Value;
        public static explicit operator StringField(string value) => new StringField { Value = value };
    }
}
