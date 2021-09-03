using System;
using System.Collections.Generic;
using System.Text;

using UniRx;

namespace Origine
{
    public abstract class BaseField : Binary
    {
        public string FieldName;

        #region operator

        public static implicit operator int(BaseField field) => (field as IntField).Value;
        public static implicit operator List<int>(BaseField field) => (field as RepeatIntField).Value;
        public static implicit operator float(BaseField field) => (field as FloatField).Value;
        public static implicit operator List<float>(BaseField field) => (field as RepeatFloatField).Value;
        public static implicit operator double(BaseField field) => (field as DoubleField).Value;
        public static implicit operator List<double>(BaseField field) => (field as RepeatDoubleField).Value;
        public static implicit operator long(BaseField field) => (field as LongField).Value;
        public static implicit operator List<long>(BaseField field) => (field as RepeatLongField).Value;
        public static implicit operator string(BaseField field) => (field as StringField).Value;
        public static implicit operator List<string> (BaseField field) => (field as RepeatStringField).Value;
        public static implicit operator bool(BaseField field) => (field as BooleanField).Value;

        #endregion
    }
}
