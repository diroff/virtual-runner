using System;
using Data.PowerUps;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Lifetimes
{
    public abstract class PowerUpLifetimeBase : MonoBehaviour
    {
        public bool IsRunning { get; private set; }

        public event Action OnExpired;
        public event Action OnChanged;

        public bool StartLifetime(PowerUpLevelSettings settings)
        {
            StopLifetime();

            if (!CanStart(settings))
                return false;

            IsRunning = true;
            OnStart(settings);
            NotifyChanged();
            return true;
        }

        public void Refresh(PowerUpLevelSettings settings)
        {
            if (IsRunning)
            {
                OnRefresh(settings);
                NotifyChanged();
            }
        }

        public void StopLifetime()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            OnStop();
            NotifyChanged();
        }

        protected void Expire()
        {
            if (!IsRunning)
                return;

            StopLifetime();
            OnExpired?.Invoke();
        }

        protected void NotifyChanged() => OnChanged?.Invoke();

        protected abstract bool CanStart(PowerUpLevelSettings settings);
        protected abstract void OnStart(PowerUpLevelSettings settings);
        protected abstract void OnRefresh(PowerUpLevelSettings settings);
        protected abstract void OnStop();

        protected virtual void OnDisable() => Expire();
    }
}
