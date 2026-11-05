using Abstractions.Components;
using Gameplay.Mechanics.Creatures;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Mechanics.Lifecycle
{
    public class GameplayClassicLevelFinisher : GameplayLevelFinisher
    {
        private HealthComponent _playerHealth;

        protected override void Awake()
        {
            base.Awake();

            _playerHealth = Player.GetComponentInChildren<HealthComponent>();
            _playerHealth.OnDied += OnPlayerDied;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(_playerHealth != null )
                _playerHealth.OnDied -= OnPlayerDied;
        }

        private void OnPlayerDied(GameObject gameObject, GameObject killer)
        {
            ExecuteFinishAsync().Forget();
        }
    }
}