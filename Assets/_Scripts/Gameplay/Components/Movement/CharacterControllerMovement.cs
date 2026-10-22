
using Abstractions.Components.Movement;
using UnityEngine;

namespace Gameplay.Components.Movement
{
    public class CharacterControllerMovement : MovementBase
    {
        [Header("Character Controller")]
        [SerializeField] private CharacterController _characterController;

        protected override bool IsConfigured => _characterController != null && _characterController.enabled;

        protected override void Awake()
        {
            if (_characterController == null)
                _characterController = GetComponent<CharacterController>();

            base.Awake();
        }

        protected override void ApplyMovement(Vector3 displacement, float deltaTime)
        {
            if (_characterController == null || !_characterController.enabled)
                return;

            _characterController.Move(displacement);
        }
    }
}
