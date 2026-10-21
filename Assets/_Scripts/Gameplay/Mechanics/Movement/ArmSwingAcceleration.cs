using Gameplay.Components.Movement;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Mechanics.Movement
{
    public class ArmSwingAcceleration : MonoBehaviour
    {
        [Header("Hand Tracking")]
        [SerializeField] private TransformVelocity _leftHandVelocity;
        [SerializeField] private TransformVelocity _rightHandVelocity;

        [Header("Speed")]
        [SerializeField] private MovementSpeed _movementSpeed;
        [SerializeField, Min(0f)] private float _minimumHandSpeed = 0.5f;
        [SerializeField, Min(0.01f)] private float _maximumHandSpeed = 2.5f;
        [SerializeField, Min(0f)] private float _maximumAdditionalSpeed = 4f;

        public float CurrentAdditionalSpeed { get; private set; }

        public UnityEvent<float> OnAdditionalSpeedChanged = new();

        private float _leftHandSpeed;
        private float _rightHandSpeed;

        private bool _leftTrackingStartedHere;
        private bool _rightTrackingStartedHere;

        private void OnEnable()
        {
            if (_movementSpeed == null)
            {
                Debug.LogError($"{nameof(ArmSwingAcceleration)}: MovementSpeed is not assigned.", this);
                return;
            }

            SubscribeToLeftHand();
            SubscribeToRightHand();
            UpdateAdditionalSpeed();
        }

        private void OnDisable()
        {
            if (_leftHandVelocity != null)
                _leftHandVelocity.OnSpeedUpdated.RemoveListener(OnLeftHandSpeedUpdated);

            if (_rightHandVelocity != null)
                _rightHandVelocity.OnSpeedUpdated.RemoveListener(OnRightHandSpeedUpdated);

            if (_leftTrackingStartedHere)
                _leftHandVelocity.StopTracking();

            if (_rightTrackingStartedHere)
                _rightHandVelocity.StopTracking();

            _leftTrackingStartedHere = false;
            _rightTrackingStartedHere = false;

            if (_movementSpeed != null)
                _movementSpeed.RemoveContribution(this);

            _leftHandSpeed = 0f;
            _rightHandSpeed = 0f;
            CurrentAdditionalSpeed = 0f;
        }

        private void SubscribeToLeftHand()
        {
            if (_leftHandVelocity == null)
                return;

            _leftHandVelocity.OnSpeedUpdated.AddListener(OnLeftHandSpeedUpdated);
            _leftHandSpeed = _leftHandVelocity.Speed;

            if (!_leftHandVelocity.IsTracking)
                _leftTrackingStartedHere = _leftHandVelocity.StartTracking();
        }

        private void SubscribeToRightHand()
        {
            if (_rightHandVelocity == null)
                return;

            _rightHandVelocity.OnSpeedUpdated.AddListener(OnRightHandSpeedUpdated);
            _rightHandSpeed = _rightHandVelocity.Speed;

            if (!_rightHandVelocity.IsTracking)
                _rightTrackingStartedHere = _rightHandVelocity.StartTracking();
        }

        private void OnLeftHandSpeedUpdated(float speed)
        {
            _leftHandSpeed = speed;
            UpdateAdditionalSpeed();
        }

        private void OnRightHandSpeedUpdated(float speed)
        {
            _rightHandSpeed = speed;
            UpdateAdditionalSpeed();
        }

        private void UpdateAdditionalSpeed()
        {
            int handCount = 0;
            float totalHandSpeed = 0f;

            if (_leftHandVelocity != null)
            {
                totalHandSpeed += _leftHandSpeed;
                handCount++;
            }

            if (_rightHandVelocity != null)
            {
                totalHandSpeed += _rightHandSpeed;
                handCount++;
            }

            float averageHandSpeed = handCount > 0 ? totalHandSpeed / handCount: 0f;
            float normalizedSpeed = Mathf.InverseLerp( _minimumHandSpeed, _maximumHandSpeed, averageHandSpeed);
            float additionalSpeed = normalizedSpeed * _maximumAdditionalSpeed;

            if (Mathf.Approximately(CurrentAdditionalSpeed, additionalSpeed))
                return;

            CurrentAdditionalSpeed = additionalSpeed;
            _movementSpeed.SetContribution(this, CurrentAdditionalSpeed);
            OnAdditionalSpeedChanged.Invoke(CurrentAdditionalSpeed);
        }

        private void OnValidate()
        {
            _minimumHandSpeed = Mathf.Max(0f, _minimumHandSpeed);
            _maximumHandSpeed = Mathf.Max(_minimumHandSpeed + 0.01f, _maximumHandSpeed);
            _maximumAdditionalSpeed = Mathf.Max(0f, _maximumAdditionalSpeed);
        }
    }
}