using System.Collections.Generic;
using UnityEngine;

namespace Common.Components.UI.Animations.Buttons
{
    [DisallowMultipleComponent]
    public sealed class UIButtonScaleMixer : MonoBehaviour
    {
        private readonly Dictionary<object, float> _multipliers = new();
        private Vector3 _initialScale;

        public void Register(object owner)
        {
            if (_multipliers.Count == 0)
                _initialScale = transform.localScale;

            _multipliers[owner] = 1f;
            ApplyScale();
        }

        public void SetMultiplier(object owner, float multiplier)
        {
            if (!_multipliers.ContainsKey(owner))
                return;

            _multipliers[owner] = multiplier;
            ApplyScale();
        }

        public void Unregister(object owner)
        {
            if (_multipliers.Remove(owner))
                ApplyScale();
        }

        private void ApplyScale()
        {
            float multiplier = 1f;

            foreach (var value in _multipliers.Values)
                multiplier *= value;

            transform.localScale = _initialScale * multiplier;
        }
    }
}
