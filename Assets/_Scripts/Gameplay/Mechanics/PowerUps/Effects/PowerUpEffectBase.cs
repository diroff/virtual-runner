using Data.PowerUps;
using Gameplay.Mechanics.PowerUps.Lifetimes;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Effects
{
    public abstract class PowerUpEffectBase : MonoBehaviour
    {
        public bool IsRunning { get; private set; }

        public bool StartEffect(PowerUpContext context, PowerUpLevelSettings settings, PowerUpLifetimeBase lifetime)
        {
            StopEffect();
            if (!OnStart(context, settings, lifetime))
                return false;

            IsRunning = true;
            return true;
        }

        public void StopEffect()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            OnStop();
        }

        protected abstract bool OnStart(PowerUpContext context, PowerUpLevelSettings settings, PowerUpLifetimeBase lifetime);
        protected abstract void OnStop();

        protected virtual void OnDisable() => StopEffect();
    }
}
