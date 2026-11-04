using Gameplay.Mechanics.PowerUps.Projectiles;
using Reflex.Attributes;
using Services;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Weapons
{
    public sealed class ProjectileBlaster : BlasterBase
    {
        [SerializeField] private Transform _muzzle;
        [SerializeField] private ProjectileBase _projectilePrefab;
        [SerializeField, Min(0.1f)] private float _projectileSpeed = 28f;
        [SerializeField, Min(0.1f)] private float _projectileRange = 30f;
        [SerializeField, Min(0.01f)] private float _projectileDamage = 1f;

        [Inject] private PoolService _poolService;

        protected override bool CanStart()
        {
            if (_muzzle != null && _projectilePrefab != null && _poolService != null)
                return true;

            Debug.LogError($"{nameof(ProjectileBlaster)}: Missing muzzle, projectile prefab or PoolService.", this);
            return false;
        }

        protected override bool Shoot(PowerUpContext context)
        {
            ProjectileBase projectile = _poolService.Get(_projectilePrefab,
                instance => instance.transform.SetPositionAndRotation(_muzzle.position, _muzzle.rotation));
            if (projectile == null)
                return false;

            projectile.Launch(new ProjectileShot(context.Player, context.Score, _poolService,
                _projectileSpeed, _projectileRange, _projectileDamage));
            return projectile.IsFlying;
        }
    }
}
