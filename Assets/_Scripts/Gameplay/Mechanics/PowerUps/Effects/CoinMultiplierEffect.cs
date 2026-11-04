using Data.PowerUps;
using Gameplay.Mechanics.Collectables;
using Gameplay.Mechanics.PowerUps.Lifetimes;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Effects
{
    public sealed class CoinMultiplierEffect : PowerUpEffectBase
    {
        [SerializeField] private CoinCollectionHandler _coinCollectionHandler;
        [SerializeField, Min(1f)] private float _multiplier = 2f;

        protected override bool OnStart(PowerUpContext context, PowerUpLevelSettings settings, PowerUpLifetimeBase lifetime)
        {
            if (_coinCollectionHandler == null)
                return false;

            _coinCollectionHandler.SetMultiplierContribution(this, Mathf.Max(1f, _multiplier));
            return true;
        }

        protected override void OnStop()
        {
            if (_coinCollectionHandler != null)
                _coinCollectionHandler.RemoveMultiplierContribution(this);
        }
    }
}
