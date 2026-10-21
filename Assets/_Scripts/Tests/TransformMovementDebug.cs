using Abstractions.Components.Movement;
using UnityEngine;

namespace Tests
{
    public class TransformMovementDebug : MonoBehaviour
    {
        [SerializeField] private MovementBase _movement;

        private void Start()
        {
            if (_movement == null)
            {
                Debug.LogError($"{nameof(TransformMovementDebug)}: Movement is not assigned.", this);
                return;
            }

            _movement.StartMovement();
        }

        private void OnDisable()
        {
            if (_movement != null)
                _movement.StopMovement();
        }
    }
}