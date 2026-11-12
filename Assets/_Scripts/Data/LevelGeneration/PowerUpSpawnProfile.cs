using System;
using UnityEngine;

namespace Data.LevelGeneration
{
    [CreateAssetMenu(menuName = "Data/Level Generation/Power Up Spawn Profile")]
    public sealed class PowerUpSpawnProfile : ScriptableObject
    {
        [Serializable]
        public struct Keyframe
        {
            [SerializeField, Min(0f)] private float _distance;
            [SerializeField, Range(0f, 1f)] private float _chance;

            public float Distance => _distance;
            public float Chance => _chance;

            public void Validate()
            {
                _distance = Mathf.Max(0f, _distance);
                _chance = Mathf.Clamp01(_chance);
            }
        }

        [SerializeField] private Keyframe[] _keyframes = Array.Empty<Keyframe>();

        public float Evaluate(float distance)
        {
            if (_keyframes == null || _keyframes.Length == 0)
                return 0f;

            if (distance <= _keyframes[0].Distance)
                return _keyframes[0].Chance;

            for (int i = 1; i < _keyframes.Length; i++)
            {
                if (distance > _keyframes[i].Distance)
                    continue;

                Keyframe previous = _keyframes[i - 1];
                Keyframe next = _keyframes[i];
                float length = next.Distance - previous.Distance;
                float t = length > 0f ? (distance - previous.Distance) / length : 1f;
                return Mathf.Lerp(previous.Chance, next.Chance, t);
            }

            return _keyframes[_keyframes.Length - 1].Chance;
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
