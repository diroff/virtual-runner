using Abstractions.Components.Movement;
using UnityEngine;

namespace Gameplay.Components.Movement
{
    public class TransformMovement : MovementBase
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        protected override bool IsConfigured => _target != null;

        protected override void Awake()
        {
            if (_target == null)
                _target = transform;

            base.Awake();
        }

        protected override void ApplyMovement(Vector3 displacement, float deltaTime)
        {
            if (displacement.sqrMagnitude <= Mathf.Epsilon)
                return;

            _target.position += displacement;
        }
    }
}