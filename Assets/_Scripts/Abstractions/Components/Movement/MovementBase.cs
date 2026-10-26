using Abstractions.Interfaces;
using Gameplay.Components.Movement;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Abstractions.Components.Movement
{
    public abstract class MovementBase : MonoBehaviour, IMovement
    {
        [Header("Movement")]
        [SerializeField] private MovementSpeed _movementSpeed;
        [SerializeField] private Transform _directionReference;
        [SerializeField] private Vector3 _direction = Vector3.forward;

        public bool IsMoving { get; private set; }

        public UnityEvent OnMovementStarted = new();
        public UnityEvent OnMovementStopped = new();
        public event Action<float> OnDistanceMoved;

        protected MovementSpeed Speed => _movementSpeed;

        protected virtual void Awake()
        {
            if (_movementSpeed == null)
                _movementSpeed = GetComponent<MovementSpeed>();
        }

        protected virtual void OnDisable()
        {
            StopMovement();
        }

        protected virtual void Update()
        {
            float deltaTime = Time.deltaTime;

            if (deltaTime <= 0f)
                return;

            Vector3 displacement = IsMoving ? GetDirection() * _movementSpeed.CurrentSpeed * deltaTime : Vector3.zero;

            ApplyMovement(displacement, deltaTime);

            if (IsMoving && displacement.sqrMagnitude > 0f)
                OnDistanceMoved?.Invoke(displacement.magnitude);
        }

        protected abstract bool IsConfigured { get; }

        protected abstract void ApplyMovement(Vector3 displacement, float deltaTime);

        public bool StartMovement()
        {
            if (_movementSpeed == null || !IsConfigured)
            {
                Debug.LogError($"{GetType().Name}: MovementSpeed or movement target is not configured.", this);

                return false;
            }

            if (IsMoving)
                return true;

            IsMoving = true;
            OnMovementStarted.Invoke();

            return true;
        }

        public void StopMovement()
        {
            if (!IsMoving)
                return;

            IsMoving = false;
            OnMovementStopped.Invoke();
        }

        public void SetDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                Debug.LogWarning($"{GetType().Name}: Direction cannot be zero.", this);
                return;
            }

            _direction = direction.normalized;
        }

        public void SetDirectionReference(Transform reference)
        {
            _directionReference = reference;
        }

        private Vector3 GetDirection()
        {
            Vector3 direction = _directionReference != null ? _directionReference.TransformDirection(_direction) : _direction;

            return direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : Vector3.zero;
        }
    }
}