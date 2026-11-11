using System;
using System.Collections.Generic;
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

        private readonly List<Collider> _colliders = new();
        private readonly List<bool> _initialColliderStates = new();

        private Action _deathPresentation;
        private bool _hasCollided;

        public event Action<GameObject> OnDied;

        protected override void Awake()
        {
            if (_health == null)
                _health = GetComponent<HealthComponent>();

            GetComponentsInChildren(true, _colliders);
            foreach (Collider collider in _colliders)
                _initialColliderStates.Add(collider.enabled);

            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_health != null)
                _health.OnDied += HandleDied;
        }

        protected override void OnDisable()
        {
            if (_health != null)
                _health.OnDied -= HandleDied;
            base.OnDisable();
        }

        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor) && !_hasCollided && _health != null && !_health.IsDead;
        }

        public void RegisterDeathPresentation(Action presentation)
        {
            _deathPresentation = presentation;
        }

        public void UnregisterDeathPresentation(Action presentation)
        {
            if (_deathPresentation == presentation)
                _deathPresentation = null;
        }

        protected override void HandleInteraction(GameObject interactor)
        {
            _hasCollided = true;

            IDamageable damageable = interactor.GetComponentInParent<IDamageable>();
            damageable?.Damage(_damage, gameObject);

            if (_health != null && !_health.IsDead)
                _health.Damage(Mathf.Max(_health.CurrentHealth, 0.01f), interactor);
        }

        protected override void ResetState()
        {
            base.ResetState();
            _hasCollided = false;

            for (int i = 0; i < _colliders.Count; i++)
                if (_colliders[i] != null)
                    _colliders[i].enabled = _initialColliderStates[i];

            _health?.ResetHealth();
        }

        private void HandleDied(GameObject victim, GameObject killer)
        {
            foreach (Collider collider in _colliders)
                if (collider != null)
                    collider.enabled = false;

            OnDied?.Invoke(killer);

            if (_health != null && !_health.IsDead)
                return;

            if (!gameObject.activeSelf)
                return;

            if (_deathPresentation != null)
                _deathPresentation.Invoke();
            else
                Hide();
        }
    }
}
