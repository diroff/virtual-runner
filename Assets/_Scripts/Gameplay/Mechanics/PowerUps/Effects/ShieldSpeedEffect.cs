using Data.PowerUps;
using Gameplay.Components.Movement;
using Gameplay.Mechanics.PowerUps.Lifetimes;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Effects
{
    public sealed class ShieldSpeedEffect : PowerUpEffectBase
    {
        [SerializeField] private MovementSpeed _movementSpeed;
        [SerializeField, Min(0f)] private float _additionalSpeed = 0.75f;

        protected override bool OnStart(PowerUpContext context, PowerUpLevelSettings settings, PowerUpLifetimeBase lifetime)
        {
            if (_movementSpeed == null)
                return false;

            _movementSpeed.SetContribution(this, _additionalSpeed);
            return true;
        }

        protected override void OnStop()
        {
            if (_movementSpeed != null)
                _movementSpeed.RemoveContribution(this);
        }
    }
}
