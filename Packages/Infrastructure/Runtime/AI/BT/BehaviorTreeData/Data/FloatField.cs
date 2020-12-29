﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Origine
{
    public partial class FloatField : BaseField
    {
        public float Value;

        public override void Read(ref Reader reader)
        {
            reader.Read(ref FieldName).Read(ref Value);
        }

        public override void Write(ref Writer writer)
        {
            writer.Write(FieldName).Write(Value);
        }

        #region operator

        public static implicit operator float(FloatField field)
        {
            return field.Value;
        }

        public static explicit operator FloatField(float value)
        {
            return new FloatField { Value = value };
        }

        public override bool Equals(object other)
        {
            if (other is float)
            {
                float field = (float)other;
                return this.Value.Equals(field);
            }
            else if (other is FloatField)
            {
                FloatField field = (FloatField)other;
                return this.Value.Equals(field.Value);
            }

            return false;
        }

        #endregion
    }
}
