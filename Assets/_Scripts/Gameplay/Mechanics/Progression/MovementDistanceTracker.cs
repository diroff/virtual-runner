using System;
using Abstractions.Components.Movement;
using Gameplay.Mechanics.LevelGeneration;
using UnityEngine;

namespace Gameplay.Mechanics.Progression
{
    public sealed class MovementDistanceTracker : MonoBehaviour
    {
        [SerializeField] private MovementBase _movement;
        [SerializeField] private LevelSegmentSpawner _segmentSpawner;

        private float _distance;
        private bool _isTracking;

        public float Distance => _distance;
        public bool IsTracking => _isTracking;

        public event Action<float> OnDistanceChanged;

        private void OnEnable()
        {
            if (_movement == null || _segmentSpawner == null)
            {
                Debug.LogError($"{nameof(MovementDistanceTracker)}: Movement or segment spawner is not assigned.", this);
                return;
            }

            _segmentSpawner.OnRunStarted += HandleRunStarted;
            _segmentSpawner.OnRunStopped += HandleRunStopped;
            _movement.OnDistanceMoved += HandleDistanceMoved;

            _isTracking = _segmentSpawner.IsRunning;
        }

        private void OnDisable()
        {
            if (_segmentSpawner != null)
            {
                _segmentSpawner.OnRunStarted -= HandleRunStarted;
                _segmentSpawner.OnRunStopped -= HandleRunStopped;
            }

            if (_movement != null)
                _movement.OnDistanceMoved -= HandleDistanceMoved;

            _isTracking = false;
        }

        private void HandleRunStarted()
        {
            _distance = 0f;
            _isTracking = true;
            OnDistanceChanged?.Invoke(_distance);
        }

        private void HandleRunStopped()
        {
            _isTracking = false;
        }

        private void HandleDistanceMoved(float distance)
        {
            if (!_isTracking || float.IsNaN(distance) || float.IsInfinity(distance) || distance <= 0f)
                return;

            _distance += distance;
            OnDistanceChanged?.Invoke(_distance);
        }
    }
}
