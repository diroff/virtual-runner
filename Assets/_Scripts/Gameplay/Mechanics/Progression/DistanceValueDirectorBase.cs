using System;
using Abstractions.Components.Movement;
using Data.Progression;
using UnityEngine;

namespace Gameplay.Mechanics.Progression
{
    public abstract class DistanceValueDirectorBase : MonoBehaviour
    {
        [SerializeField] private MovementBase _movement;
        [SerializeField] private DistanceValueProfile _profile;

        private float _distance;
        private float _currentValue;
        private bool _isRunning;

        public bool IsRunning => _isRunning;
        public float Distance => _distance;
        public float CurrentValue => _currentValue;

        public event Action<float> OnValueChanged;

        private void OnEnable()
        {
            if (_movement == null)
                return;

            _movement.OnMovementStarted.AddListener(HandleMovementStarted);
            _movement.OnMovementStopped.AddListener(StopProgression);

            if (_movement.IsMoving)
                StartProgression();
        }

        private void OnDisable()
        {
            if (_movement != null)
            {
                _movement.OnMovementStarted.RemoveListener(HandleMovementStarted);
                _movement.OnMovementStopped.RemoveListener(StopProgression);
            }

            StopProgression();
        }

        public bool StartProgression()
        {
            if (_isRunning)
                return true;

            if (_movement == null || _profile == null || !_profile.HasKeyframes)
            {
                Debug.LogError($"{GetType().Name}: Movement or profile is not configured.", this);
                return false;
            }

            if (!OnStart())
                return false;

            _distance = 0f;
            _currentValue = float.NaN;
            _isRunning = true;
            _movement.OnDistanceMoved += HandleDistanceMoved;
            UpdateValue();
            return true;
        }

        public void StopProgression()
        {
            if (!_isRunning)
                return;

            _movement.OnDistanceMoved -= HandleDistanceMoved;
            _isRunning = false;
            OnStop();
        }

        protected abstract bool OnStart();
        protected abstract void ApplyValue(float value);
        protected abstract void OnStop();

        private void HandleMovementStarted()
        {
            StartProgression();
        }

        private void HandleDistanceMoved(float distance)
        {
            if (!_isRunning || float.IsNaN(distance) || float.IsInfinity(distance) || distance <= 0f)
                return;

            _distance += distance;
            UpdateValue();
        }

        private void UpdateValue()
        {
            float value = _profile.Evaluate(_distance);

            if (Mathf.Approximately(_currentValue, value))
                return;

            _currentValue = value;
            ApplyValue(value);
            OnValueChanged?.Invoke(value);
        }
    }
}
