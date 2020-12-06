using System;
using System.Collections.Generic;

namespace Origine
{
    public class FillableValueEqualityComparer : IEqualityComparer<FillableValue>
    {
        public bool Equals(FillableValue x, FillableValue y) => x.Value == y.Value && x.Max == y.Max;

        public int GetHashCode(FillableValue obj) => obj.Value << 4 | obj.Max;
    }

    /// <summary>
    /// 可填充的值
    /// </summary>
    [Serializable]
    public class FillableValue
    {
        public int Max { get; private set; }
        public int Used => Max - Value;

        public int Value
        {
            get => current;
            private set
            {
                var realValue = value > 0 ? Math.Min(value, Max) : Math.Max(value, 0);
                var diff = realValue - current;
                if (diff != 0) OnValueChanged?.Invoke(this, diff);
                current = realValue;
            }
        }

        private int current;

        public event EventHandler<int> OnValueChanged;

        public float Percent => Value / (float)Max;

        public FillableValue(int value)
        {
            Max = value;
            current = value;
        }

        public bool IsFull => Value == Max;
        public bool IsEmpty => Value <= 0;

        public void SetFull() => Value = Max;

        public void SetEmpty() => Value = 0;

        public void SetValue(int value)
        {
            if (value > Max) Value = Max;
            else if (value < 0) Value = 0;
            else Value = value;
        }

        public void SetMax(int max) => Max = max;

        public static implicit operator int(FillableValue value) => value.Value;

        public static FillableValue operator +(FillableValue lhs, int rhs)
        {
            lhs.Value += rhs;
            if (lhs.Max < lhs.Value)
                lhs.Value = lhs.Max;
            return lhs;
        }

        public static FillableValue operator -(FillableValue lhs, int rhs)
        {
            lhs.Value -= rhs;
            if (lhs.Value < 0)
                lhs.Value = 0;
            return lhs;
        }

        public string ToPercentString(int digits = 2) => $"{Math.Round(Percent, digits + 2) * 100}%";

        public override string ToString() => $"{Value}/{Max}";
    }
}