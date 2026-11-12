using System;
using Data.Progression;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Mechanics.Progression
{
    public abstract class DistanceValueDirectorBase : MonoBehaviour
    {
        [Inject] private MovementDistanceTracker _distanceTracker;
        [SerializeField] private DistanceValueProfile _profile;

        private float _currentValue;
        private bool _isRunning;

        public bool IsRunning => _isRunning;
        public float Distance => _distanceTracker != null ? _distanceTracker.Distance : 0f;
        public float CurrentValue => _currentValue;

        public event Action<float> OnValueChanged;

        private void OnEnable()
        {
            if (_distanceTracker == null)
            {
                Debug.LogError($"{GetType().Name}: MovementDistanceTracker is not registered.", this);
                return;
            }

            _distanceTracker.OnTrackingStarted += HandleTrackingStarted;
            _distanceTracker.OnTrackingStopped += StopProgression;
            _distanceTracker.OnDistanceChanged += HandleDistanceChanged;

            if (_distanceTracker.IsTracking)
                StartProgression();
        }

        private void OnDisable()
        {
            if (_distanceTracker != null)
            {
                _distanceTracker.OnTrackingStarted -= HandleTrackingStarted;
                _distanceTracker.OnTrackingStopped -= StopProgression;
                _distanceTracker.OnDistanceChanged -= HandleDistanceChanged;
            }

            StopProgression();
        }

        public bool StartProgression()
        {
            if (_isRunning)
                return true;

            if (_distanceTracker == null || _profile == null || !_profile.HasKeyframes)
            {
                Debug.LogError($"{GetType().Name}: Distance tracker or profile is not configured.", this);
                return false;
            }

            if (!OnStart())
                return false;

            _currentValue = float.NaN;
            _isRunning = true;
            UpdateValue(_distanceTracker.Distance);
            return true;
        }

        public void StopProgression()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            OnStop();
        }

        protected abstract bool OnStart();
        protected abstract void ApplyValue(float value);
        protected abstract void OnStop();

        private void HandleTrackingStarted()
        {
            StartProgression();
        }

        private void HandleDistanceChanged(float distance)
        {
            if (_isRunning)
                UpdateValue(distance);
        }

        private void UpdateValue(float distance)
        {
            float value = _profile.Evaluate(distance);

            if (Mathf.Approximately(_currentValue, value))
                return;

            _currentValue = value;
            ApplyValue(value);
            OnValueChanged?.Invoke(value);
        }
    }
}
