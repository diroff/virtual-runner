using System;
using Abstractions.Components.Movement;
using Gameplay.Mechanics.LevelGeneration;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Mechanics.Progression
{
    public sealed class MovementDistanceTracker : MonoBehaviour
    {
        [Inject] private MovementBase _movement;
        [Inject] private LevelSegmentSpawner _segmentSpawner;

        private float _distance;
        private bool _isTracking;

        public float Distance => _distance;
        public bool IsTracking => _isTracking;

        public event Action<float> OnDistanceChanged;
        public event Action<float> OnDistanceAdvanced;
        public event Action OnTrackingStarted;
        public event Action OnTrackingStopped;

        private void OnEnable()
        {
            if (_movement == null || _segmentSpawner == null)
            {
                Debug.LogError($"{nameof(MovementDistanceTracker)}: Movement or segment spawner is not registered.", this);
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

            if (_isTracking)
            {
                _isTracking = false;
                OnTrackingStopped?.Invoke();
            }
        }

        private void HandleRunStarted()
        {
            _distance = 0f;
            _isTracking = true;
            OnDistanceChanged?.Invoke(_distance);
            if (_isTracking)
                OnTrackingStarted?.Invoke();
        }

        private void HandleRunStopped()
        {
            if (!_isTracking)
                return;

            _isTracking = false;
            OnTrackingStopped?.Invoke();
        }

        private void HandleDistanceMoved(float distance)
        {
            if (!_isTracking || float.IsNaN(distance) || float.IsInfinity(distance) || distance <= 0f)
                return;

            _distance += distance;
            OnDistanceChanged?.Invoke(_distance);
            if (_isTracking)
                OnDistanceAdvanced?.Invoke(distance);
        }
    }
}
