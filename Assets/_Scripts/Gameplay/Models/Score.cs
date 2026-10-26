using System;

namespace Gameplay.Models
{
    public class Score
    {
        public long Value { get; private set; }

        public event Action<long, long> OnValueChanged;

        public Score(long initialValue = 0)
        {
            Value = Math.Max(0, initialValue);
        }

        public void Add(long amount)
        {
            if (amount <= 0)
                return;

            SetValue(checked(Value + amount));
        }

        public void Remove(long amount)
        {
            if (amount <= 0)
                return;

            SetValue(Math.Max(0, Value - amount));
        }

        public void SetValue(long value)
        {
            value = Math.Max(0, value);

            if (Value == value)
                return;

            long previousValue = Value;
            Value = value;

            OnValueChanged?.Invoke(previousValue, Value);
        }

        public void Reset()
        {
            SetValue(0);
        }
    }
}