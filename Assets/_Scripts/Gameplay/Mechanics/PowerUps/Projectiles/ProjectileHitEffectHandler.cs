using System.Collections.Generic;
using Gameplay.Components.Animations;
using Reflex.Attributes;
using Services;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Projectiles
{
    public sealed class ProjectileHitEffectHandler : MonoBehaviour
    {
        [SerializeField] private PooledEffectPlayer _hitEffectPlayer;

        private readonly HashSet<ProjectileBase> _projectiles = new();
        private PoolService _poolService;
        private bool _isSubscribed;

        [Inject]
        private void Initialize(PoolService poolService)
        {
            _poolService = poolService;
            Subscribe();
        }

        private void OnEnable() => Subscribe();

        private void OnDisable()
        {
            if (_isSubscribed)
            {
                _poolService.UnsubscribeFromCreated<ProjectileBase>(HandleProjectileCreated);
                _isSubscribed = false;
            }

            foreach (ProjectileBase projectile in _projectiles)
            {
                if (projectile != null)
                    projectile.OnHit -= HandleHit;
            }

            _projectiles.Clear();
        }

        private void Subscribe()
        {
            if (!isActiveAndEnabled || _poolService == null || _isSubscribed)
                return;

            _isSubscribed = true;
            _poolService.SubscribeToCreated<ProjectileBase>(HandleProjectileCreated);
        }

        private void HandleProjectileCreated(ProjectileBase projectile)
        {
            if (projectile == null || !_projectiles.Add(projectile))
                return;

            projectile.OnHit += HandleHit;
        }

        private void HandleHit(Collider collider, Vector3 position)
        {
            if (_hitEffectPlayer != null)
                _hitEffectPlayer.Play(position);
        }
    }
}
