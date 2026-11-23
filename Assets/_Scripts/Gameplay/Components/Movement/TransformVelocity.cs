using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Components.Movement
{
    public class TransformVelocity : MonoBehaviour
    {
        [Header("Tracking")]
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _reference;

        // Velocity remains in reference-local coordinates for existing consumers.
        public Vector3 Velocity { get; private set; }
        public float Speed { get; private set; }
        public bool IsTracking { get; private set; }

        // Reference-relative velocity expressed on world axes. Useful when the reference is tilted.
        public Vector3 VelocityInWorldAxes => _reference != null
            ? _reference.TransformVector(Velocity)
            : Velocity;

        public UnityEvent<Vector3> OnVelocityUpdated = new();
        public UnityEvent<float> OnSpeedUpdated = new();

        private readonly HashSet<Object> _trackingOwners = new();
        private bool _legacyTrackingRequested;
        private Vector3 _previousPosition;

        // Kept for backwards compatibility: this explicitly restarts the measurement,
        // just like the original StartTracking method did.
        public bool StartTracking()
        {
            if (!ValidateTarget())
                return false;

            _legacyTrackingRequested = true;
            BeginTracking();
            return true;
        }

        // Releases only the legacy request. Other features holding a tracking lease
        // must not lose tracking when one consumer stops.
        public void StopTracking()
        {
            _legacyTrackingRequested = false;
            StopIfUnused();
        }

        // Prefer these methods when multiple independent features share a tracker.
        // Acquiring an already-active tracker does not reset its velocity samples.
        public bool AcquireTracking(Object owner)
        {
            if (owner == null)
            {
                Debug.LogError($"{nameof(TransformVelocity)}: Tracking owner is null.", this);
                return false;
            }

            if (!ValidateTarget())
                return false;

            _trackingOwners.Add(owner);

            if (!IsTracking)
                BeginTracking();

            return true;
        }

        public void ReleaseTracking(Object owner)
        {
            // Unity destroyed objects compare == null; still allow removing their lease.
            if (ReferenceEquals(owner, null))
                return;

            _trackingOwners.Remove(owner);
            StopIfUnused();
        }

        private bool ValidateTarget()
        {
            if (_target != null)
                return true;

            Debug.LogError($"{nameof(TransformVelocity)}: Target is not assigned.", this);
            return false;
        }

        private void BeginTracking()
        {
            _previousPosition = GetPosition();
            Velocity = Vector3.zero;
            Speed = 0f;
            IsTracking = true;

            OnVelocityUpdated.Invoke(Velocity);
            OnSpeedUpdated.Invoke(Speed);
        }

        private void StopIfUnused()
        {
            if (!IsTracking || _legacyTrackingRequested || _trackingOwners.Count > 0)
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
