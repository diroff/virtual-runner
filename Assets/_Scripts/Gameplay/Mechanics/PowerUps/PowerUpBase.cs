using System;
using Data.PowerUps;
using Gameplay.Mechanics.PowerUps.Effects;
using Gameplay.Mechanics.PowerUps.Lifetimes;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps
{
    public abstract class PowerUpBase : MonoBehaviour
    {
        [SerializeField] private PowerUpDefinition _definition;
        [SerializeField] private PowerUpLifetimeBase _lifetime;
        [SerializeField] private PowerUpEffectBase[] _effects = Array.Empty<PowerUpEffectBase>();

        public PowerUpDefinition Definition => _definition;
        public PowerUpLifetimeBase Lifetime => _lifetime;
        public bool IsRunning { get; private set; }
        public int Level { get; private set; } = 1;

        public event Action<PowerUpBase> OnEnded;
        public event Action<PowerUpBase> OnUpdated;

        protected virtual void OnDisable() => StopPowerUp();

        public bool StartPowerUp(PowerUpContext context, int level)
        {
            if (IsRunning || context == null || _definition == null || _lifetime == null || _definition.MaxLevel == 0 ||
                _effects == null || _effects.Length == 0)
                return false;

            Level = Mathf.Clamp(level, 1, _definition.MaxLevel);
            PowerUpLevelSettings settings = _definition.GetSettings(Level);

            if (!_lifetime.StartLifetime(settings))
            {
                Debug.Log("Can't start life time with settings");
                return false;
            }

            _lifetime.OnExpired += HandleExpired;
            _lifetime.OnChanged += HandleChanged;

            foreach (PowerUpEffectBase effect in _effects)
            {
                if (effect != null && effect.StartEffect(context, settings, _lifetime))
                    continue;

                StopPowerUp();
                return false;
            }

            IsRunning = true;
            OnUpdated?.Invoke(this);
            return true;
        }

        public void RefreshPowerUp()
        {
            if (!IsRunning)
                return;

            _lifetime.Refresh(_definition.GetSettings(Level));
            OnUpdated?.Invoke(this);
        }

        public void StopPowerUp()
        {
            bool wasRunning = IsRunning;
            IsRunning = false;

            if (_lifetime != null)
            {
                _lifetime.OnExpired -= HandleExpired;
                _lifetime.OnChanged -= HandleChanged;
            }

            for (int i = _effects.Length - 1; i >= 0; i--)
                if (_effects[i] != null)
                    _effects[i].StopEffect();

            if (_lifetime != null)
                _lifetime.StopLifetime();

            if (wasRunning)
                OnEnded?.Invoke(this);
        }

        private void HandleExpired() => StopPowerUp();
        private void HandleChanged() => OnUpdated?.Invoke(this);
    }
}
