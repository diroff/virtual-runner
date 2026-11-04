using System;
using Data.PowerUps;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Lifetimes
{
    public sealed class ChargePowerUpLifetime : PowerUpLifetimeBase
    {
        [SerializeField] private bool _limitRefreshToMaximum = true;

        public int Charges { get; private set; }
        public int InitialCapacity { get; private set; }

        protected override bool CanStart(PowerUpLevelSettings settings) => settings.Charges > 0;

        protected override void OnStart(PowerUpLevelSettings settings)
        {
            InitialCapacity = settings.Charges;
            Charges = settings.Charges;
        }

        protected override void OnRefresh(PowerUpLevelSettings settings)
        {
            InitialCapacity = settings.Charges;
            long updated = (long)Charges + settings.Charges;
            Charges = (int)Math.Min(_limitRefreshToMaximum ? InitialCapacity : int.MaxValue, updated);
        }

        protected override void OnStop()
        {
            Charges = 0;
            InitialCapacity = 0;
        }

        public bool TryConsume(int amount = 1)
        {
            if (!IsRunning || amount <= 0 || Charges < amount)
                return false;

            Charges -= amount;
            NotifyChanged();

            if (Charges == 0)
                Expire();

            return true;
        }
    }
}
