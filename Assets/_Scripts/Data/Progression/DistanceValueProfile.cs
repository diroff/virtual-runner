using System;
using UnityEngine;

namespace Data.Progression
{
    [CreateAssetMenu(menuName = "Data/Progression/Distance Value Profile")]
    public sealed class DistanceValueProfile : ScriptableObject
    {
        [Serializable]
        public struct Keyframe
        {
            [SerializeField, Min(0f)] private float _distance;
            [SerializeField, Min(0f)] private float _value;

            public float Distance => _distance;
            public float Value => _value;

            public void Validate()
            {
                _distance = Mathf.Max(0f, _distance);
                _value = Mathf.Max(0f, _value);
            }
        }

        [SerializeField] private Keyframe[] _keyframes = Array.Empty<Keyframe>();

        public bool HasKeyframes => _keyframes != null && _keyframes.Length > 0;

        public float Evaluate(float distance)
        {
            if (!HasKeyframes)
                return 0f;

            distance = Mathf.Max(0f, distance);

            if (distance <= _keyframes[0].Distance)
                return _keyframes[0].Value;

            for (int i = 1; i < _keyframes.Length; i++)
            {
                if (distance > _keyframes[i].Distance)
                    continue;

                Keyframe previous = _keyframes[i - 1];
                Keyframe next = _keyframes[i];
                float length = next.Distance - previous.Distance;
                float t = length > 0f ? (distance - previous.Distance) / length : 1f;
                return Mathf.Lerp(previous.Value, next.Value, t);
            }

            return _keyframes[_keyframes.Length - 1].Value;
        }

        private void OnValidate()
        {
            if (_keyframes == null)
                _keyframes = Array.Empty<Keyframe>();

            for (int i = 0; i < _keyframes.Length; i++)
            {
                Keyframe keyframe = _keyframes[i];
                keyframe.Validate();
                _keyframes[i] = keyframe;
            }

            Array.Sort(_keyframes, (a, b) => a.Distance.CompareTo(b.Distance));
        }
    }
}
