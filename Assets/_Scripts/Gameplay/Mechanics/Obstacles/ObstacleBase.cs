using Abstractions.Components;
using Abstractions.Components.Interaction;
using Abstractions.Interfaces;
using UnityEngine;

namespace Gameplay.Mechanics.Obstacles
{
    public class ObstacleBase : InteractableBase
    {
        [Header("Obstacle")]
        [SerializeField, Min(0f)] private float _damage = 1f;
        [SerializeField] private HealthComponent _health;

        private bool _hasCollided;

        protected override void Awake()
        {
            if (_health == null)
                _health = GetComponent<HealthComponent>();

            base.Awake();
        }

        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor) && !_hasCollided && (_health == null || !_health.IsDead);
        }

        protected override void HandleInteraction(GameObject interactor)
        {
            _hasCollided = true;
            var damageable = interactor.GetComponentInParent<IDamageable>();

            if (damageable != null)
                damageable.Damage(_damage, gameObject);
        }

        protected override void ResetState()
        {
            base.ResetState();
            _hasCollided = false;

            if (_health != null)
                _health.ResetHealth();
        }
    }
}