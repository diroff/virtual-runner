using System;
using Abstractions.Interfaces;
using Data.PowerUps;
using Gameplay.Mechanics.PowerUps.Lifetimes;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Effects
{
    public sealed class ShieldProtectionEffect : PowerUpEffectBase, IDamageProtection
    {
        private ChargePowerUpLifetime _charges;

        public event Action<float, GameObject> OnHitPrevented;

        protected override bool OnStart(PowerUpContext context, PowerUpLevelSettings settings, PowerUpLifetimeBase lifetime)
        {
            _charges = lifetime as ChargePowerUpLifetime;
            return _charges != null;
        }

        protected override void OnStop() => _charges = null;

        public bool TryPreventDamage(float amount, GameObject sender)
        {
            if (!IsRunning || amount <= 0f || _charges == null || !_charges.TryConsume())
                return false;

            OnHitPrevented?.Invoke(amount, sender);
            return true;
        }
    }
}
