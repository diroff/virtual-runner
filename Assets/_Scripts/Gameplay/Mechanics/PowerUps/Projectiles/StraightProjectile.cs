using System;
using System.Threading;
using Abstractions.Components;
using Abstractions.Components.Zone;
using Abstractions.Interfaces;
using Cysharp.Threading.Tasks;
using Gameplay.Mechanics.PowerUps.Weapons;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class StraightProjectile : ProjectileBase
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Zone3DBase _hitZone;

        private CancellationTokenSource _cts;

        private void Awake()
        {
            EnsureRigidbody();
            ResetRigidbody();
        }

        public override void OnCreated()
        {
            EnsureRigidbody();
            ResetRigidbody();
        }

        public override void OnSpawned()
        {
            EnsureRigidbody();
            ResetRigidbody();
            SetZoneActive(false);
        }

        protected override void OnLaunch()
        {
            EnsureRigidbody();
            if (_rigidbody == null || _hitZone == null || Shot.Speed <= 0f || Shot.Range <= 0f)
            {
                Debug.LogError($"{nameof(StraightProjectile)}: Rigidbody, Hit Zone, Speed and Range must be configured.", this);
                ReturnToPool();
                return;
            }

            _hitZone.ResetRuntimeState();
            _hitZone.OnEnter += HandleHit;
            _hitZone.IsActive = true;

            _rigidbody.useGravity = false;
            _rigidbody.linearDamping = 0f;
            _rigidbody.angularDamping = 0f;
            _rigidbody.isKinematic = false;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.linearVelocity = transform.forward * Shot.Speed;

            _cts = new CancellationTokenSource();
            ExpireAsync(Shot.Range / Shot.Speed, _cts.Token).Forget();
        }

        protected override void OnStop()
        {
            SetZoneActive(false);
            if (_hitZone != null)
                _hitZone.OnEnter -= HandleHit;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            ResetRigidbody();
        }

        private void HandleHit(Collider collider)
        {
            if (!IsFlying || ShouldIgnore(collider))
                return;

            IDamageable damageable = collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                HealthComponent health = collider.GetComponentInParent<HealthComponent>();
                ObstacleScoreReward reward = collider.GetComponentInParent<ObstacleScoreReward>();
                bool wasAlive = health != null && !health.IsDead;
                GameObject sender = Sender;
                var score = Shot.Score;
                float damage = Shot.Damage;

                damageable.Damage(damage, sender);

                if (wasAlive && health != null && health.IsDead && reward != null && score != null)
                    score.Add(reward.Points);
            }

            ReturnToPool();
        }

        private async UniTask ExpireAsync(float seconds, CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(seconds), DelayType.DeltaTime,
                    PlayerLoopTiming.FixedUpdate, token);
                if (IsFlying)
                    ReturnToPool();
            }
            catch (OperationCanceledException) { }
        }

        private void EnsureRigidbody()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();
        }

        private void ResetRigidbody()
        {
            if (_rigidbody == null)
                return;

            if (!_rigidbody.isKinematic)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }

            _rigidbody.isKinematic = true;
        }

        private void SetZoneActive(bool active)
        {
            if (_hitZone == null)
                return;

            _hitZone.IsActive = active;
            if (!active)
                _hitZone.ResetRuntimeState();
        }
    }
}
