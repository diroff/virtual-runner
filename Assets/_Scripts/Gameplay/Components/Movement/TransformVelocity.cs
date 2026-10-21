using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Components.Movement
{
    public class TransformVelocity : MonoBehaviour
    {
        [Header("Tracking")]
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _reference;

        public Vector3 Velocity { get; private set; }
        public float Speed { get; private set; }
        public bool IsTracking { get; private set; }

        public UnityEvent<Vector3> OnVelocityUpdated = new();
        public UnityEvent<float> OnSpeedUpdated = new();

        private Vector3 _previousPosition;

        public bool StartTracking()
        {
            if (_target == null)
            {
                Debug.LogError($"{nameof(TransformVelocity)}: Target is not assigned.", this);
                return false;
            }

            _previousPosition = GetPosition();
            Velocity = Vector3.zero;
            Speed = 0f;
            IsTracking = true;

            OnVelocityUpdated.Invoke(Velocity);
            OnSpeedUpdated.Invoke(Speed);

            return true;
        }

        public void StopTracking()
        {
            if (!IsTracking)
                return;

            IsTracking = false;
            Velocity = Vector3.zero;
            Speed = 0f;

            OnVelocityUpdated.Invoke(Velocity);
            OnSpeedUpdated.Invoke(Speed);
        }

        private void Update()
        {
            if (!IsTracking)
                return;

            UpdateVelocity();
            PublishVelocity();
        }

        private void UpdateVelocity()
        {
            float deltaTime = Time.deltaTime;

            if (deltaTime <= 0f)
                return;

            Vector3 currentPosition = GetPosition();
            Velocity = (currentPosition - _previousPosition) / deltaTime;
            Speed = Velocity.magnitude;
            _previousPosition = currentPosition;
        }

        private Vector3 GetPosition()
        {
            return _reference != null ? _reference.InverseTransformPoint(_target.position) : _target.position;
        }

        private void PublishVelocity()
        {
            OnVelocityUpdated.Invoke(Velocity);
            OnSpeedUpdated.Invoke(Speed);
        }
    }
}