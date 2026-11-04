using System.Collections.Generic;
using Abstraction.Interfaces;
using Services;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Projectiles
{
    public abstract class ProjectileBase : MonoBehaviour, IPoolable
    {
        private readonly List<Collider> _ownColliders = new();
        private readonly List<Collider> _senderColliders = new();
        private readonly List<(Collider First, Collider Second)> _ignoredPairs = new();

        private PoolService _pool;

        public bool IsFlying { get; private set; }
        public GameObject Sender => IsFlying ? Shot.Sender : null;
        protected ProjectileShot Shot { get; private set; }

        public void Launch(ProjectileShot shot)
        {
            StopFlight();
            Shot = shot;
            _pool = shot.Pool;
            IsFlying = true;
            IgnoreCollisions();
            OnLaunch();
        }

        public void StopFlight()
        {
            if (!IsFlying)
                return;

            IsFlying = false;
            OnStop();
            RestoreCollisions();
            Shot = default;
            _pool = null;
        }

        protected bool ShouldIgnore(Collider collider)
        {
            if (collider == null || collider.transform.IsChildOf(transform))
                return true;

            return Shot.Sender != null && collider.transform.IsChildOf(Shot.Sender.transform);
        }

        protected void ReturnToPool()
        {
            if (!IsFlying)
                return;

            PoolService pool = _pool;
            StopFlight();
            if (pool != null)
                pool.Return(this);
            else
                Destroy(gameObject);
        }

        private void IgnoreCollisions()
        {
            GetComponentsInChildren(true, _ownColliders);

            for (int i = 0; i < _ownColliders.Count; i++)
            {
                for (int j = i + 1; j < _ownColliders.Count; j++)
                    IgnoreCollisionPair(_ownColliders[i], _ownColliders[j]);
            }

            GameObject sender = Sender;
            if (sender == null)
                return;

            sender.GetComponentsInChildren(true, _senderColliders);
            foreach (Collider own in _ownColliders)
            {
                foreach (Collider other in _senderColliders)
                    IgnoreCollisionPair(own, other);
            }
        }

        private void IgnoreCollisionPair(Collider first, Collider second)
        {
            if (first == null || second == null || first == second ||
                !first.enabled || !second.enabled ||
                !first.gameObject.activeInHierarchy || !second.gameObject.activeInHierarchy ||
                Physics.GetIgnoreCollision(first, second))
                return;

            Physics.IgnoreCollision(first, second, true);
            _ignoredPairs.Add((first, second));
        }

        private void RestoreCollisions()
        {
            foreach ((Collider first, Collider second) in _ignoredPairs)
            {
                if (first != null && second != null && first.gameObject.activeInHierarchy &&
                    second.gameObject.activeInHierarchy)
                    Physics.IgnoreCollision(first, second, false);
            }

            _ignoredPairs.Clear();
            _ownColliders.Clear();
            _senderColliders.Clear();
        }

        protected abstract void OnLaunch();
        protected abstract void OnStop();

        public virtual void OnCreated() { }
        public virtual void OnSpawned() { }
        public virtual void OnDespawned() => StopFlight();

        protected virtual void OnDisable() => StopFlight();
    }
}
