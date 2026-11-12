using Gameplay.Components.Movement;
using UnityEngine;

namespace Gameplay.Mechanics.Progression
{
    public sealed class MovementSpeedDirector : DistanceValueDirectorBase
    {
        [SerializeField] private MovementSpeed _movementSpeed;

        private float _initialSpeed;

        protected override bool OnStart()
        {
            if (_movementSpeed == null)
            {
                Debug.LogError($"{nameof(MovementSpeedDirector)}: MovementSpeed is not assigned.", this);
                return false;
            }

            _initialSpeed = _movementSpeed.BaseSpeed;
            return true;
        }

        protected override void ApplyValue(float value)
        {
            _movementSpeed.SetBaseSpeed(value);
        }

        protected override void OnStop()
        {
            if (_movementSpeed != null)
                _movementSpeed.SetBaseSpeed(_initialSpeed);
        }
    }
}
