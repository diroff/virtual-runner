using Abstractions.Components.Movement;
using UnityEngine;

namespace Gameplay.Components.Movement
{
    public class CharacterControllerMovement : MovementBase
    {
        [Header("Character Controller")]
        [SerializeField] private CharacterController _characterController;

        [Header("Gravity")]
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField, Min(0f)] private float _groundedStickForce = 2f;

        [Header("Tracked Head")]
        [SerializeField] private Transform _trackedHead;
        [SerializeField] private bool _fitCapsuleToHead = true;
        [SerializeField, Min(0f)] private float _headClearance = 0.2f;
        [SerializeField, Min(0.1f)] private float _minimumHeight = 0.8f;
        [SerializeField, Min(0.1f)] private float _maximumHeight = 2.2f;

        private float _verticalVelocity;

        protected override bool IsConfigured => _characterController != null && _characterController.enabled;

        protected override void Awake()
        {
            if (_characterController == null)
                _characterController = GetComponent<CharacterController>();

            base.Awake();
        }

        protected override void Update()
        {
            if (_fitCapsuleToHead)
                FitCapsuleToHead();

            base.Update();
        }

        protected override void ApplyMovement(Vector3 displacement, float deltaTime)
        {
            if (_characterController == null || !_characterController.enabled)
                return;

            if (_characterController.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -_groundedStickForce;
            else
                _verticalVelocity += _gravity * deltaTime;

            displacement.y += _verticalVelocity * deltaTime;

            CollisionFlags collisionFlags = _characterController.Move(displacement);

            if ((collisionFlags & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
                _verticalVelocity = 0f;
        }

        private void FitCapsuleToHead()
        {
            if (_trackedHead == null || _characterController == null)
                return;

            Vector3 headPosition = transform.InverseTransformPoint(_trackedHead.position);

            float minimumHeight = Mathf.Max(_minimumHeight, _characterController.radius * 2f);
            float maximumHeight = Mathf.Max(_maximumHeight, minimumHeight);
            float height = Mathf.Clamp(headPosition.y + _headClearance, minimumHeight, maximumHeight);

            _characterController.height = height;
            _characterController.center = new Vector3(headPosition.x, height * 0.5f, headPosition.z);
        }

        private void OnValidate()
        {
            _minimumHeight = Mathf.Max(0.1f, _minimumHeight);
            _maximumHeight = Mathf.Max(_minimumHeight, _maximumHeight);
            _headClearance = Mathf.Max(0f, _headClearance);
            _groundedStickForce = Mathf.Max(0f, _groundedStickForce);
        }
    }
}