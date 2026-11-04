using Data.PowerUps;
using Gameplay.Mechanics.PowerUps.Lifetimes;
using Gameplay.Mechanics.PowerUps.Weapons;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Effects
{
    public sealed class BlasterFireEffect : PowerUpEffectBase
    {
        [SerializeField] private BlasterBase _blaster;

        protected override bool OnStart(PowerUpContext context, PowerUpLevelSettings settings, PowerUpLifetimeBase lifetime)
        {
            return _blaster != null && lifetime is ChargePowerUpLifetime charges && _blaster.StartShooting(context, charges);
        }

        protected override void OnStop()
        {
            if (_blaster != null)
                _blaster.StopShooting();
        }
    }
}
