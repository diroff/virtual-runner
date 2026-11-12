using System;
using UnityEngine;

namespace Data.LevelGeneration
{
    [CreateAssetMenu(menuName = "Data/Level Generation/Segment Difficulty Profile")]
    public sealed class SegmentDifficultyProfile : ScriptableObject
    {
        [Serializable]
        public struct Keyframe
        {
            [SerializeField, Min(0f)] private float _distance;
            [SerializeField, Min(0f)] private float _easy;
            [SerializeField, Min(0f)] private float _medium;
            [SerializeField, Min(0f)] private float _hard;

            public float Distance => _distance;
            public DifficultyWeights Weights => new(_easy, _medium, _hard);

            public void Validate()
            {
                _distance = Mathf.Max(0f, _distance);
                _easy = Mathf.Max(0f, _easy);
                _medium = Mathf.Max(0f, _medium);
                _hard = Mathf.Max(0f, _hard);
            }
        }

        public readonly struct DifficultyWeights
        {
            public readonly float Easy;
            public readonly float Medium;
            public readonly float Hard;

            public DifficultyWeights(float easy, float medium, float hard)
            {
                Easy = Mathf.Max(0f, easy);
                Medium = Mathf.Max(0f, medium);
                Hard = Mathf.Max(0f, hard);
            }

            public static DifficultyWeights Lerp(DifficultyWeights from, DifficultyWeights to, float t)
            {
                return new DifficultyWeights(
                    Mathf.Lerp(from.Easy, to.Easy, t),
                    Mathf.Lerp(from.Medium, to.Medium, t),
                    Mathf.Lerp(from.Hard, to.Hard, t));
            }
        }

        [SerializeField] private Keyframe[] _keyframes = Array.Empty<Keyframe>();

        public DifficultyWeights Evaluate(float distance)
        {
            if (_keyframes == null || _keyframes.Length == 0)
                return new DifficultyWeights(1f, 0f, 0f);

            if (distance <= _keyframes[0].Distance)
                return _keyframes[0].Weights;

            for (int i = 1; i < _keyframes.Length; i++)
            {
                if (distance > _keyframes[i].Distance)
                    continue;

                Keyframe previous = _keyframes[i - 1];
                Keyframe next = _keyframes[i];
                float length = next.Distance - previous.Distance;
                float t = length > 0f ? (distance - previous.Distance) / length : 1f;
                return DifficultyWeights.Lerp(previous.Weights, next.Weights, t);
            }

            return _keyframes[_keyframes.Length - 1].Weights;
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
