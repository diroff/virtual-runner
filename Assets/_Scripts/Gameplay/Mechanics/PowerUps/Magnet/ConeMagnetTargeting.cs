using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Magnet
{
    public sealed class ConeMagnetTargeting : MagnetTargetingBase
    {
        [SerializeField, Min(0.1f)] private float _range = 12f;
        [SerializeField, Range(1f, 180f)] private float _fullAngle = 25f;
        [SerializeField] private LayerMask _targetMask;

        public float Range => _range;
        public float FullAngle => _fullAngle;

        private Collider[] _colliders = new Collider[32];

        public override void FindTargets(Transform source, List<MagnetAttractable> results)
        {
            results.Clear();
            if (source == null)
                return;

            int count;
            while (true)
            {
                count = Physics.OverlapSphereNonAlloc(source.position, _range, _colliders, _targetMask,
                    QueryTriggerInteraction.Collide);

                if (count < _colliders.Length)
                    break;

                Array.Resize(ref _colliders, _colliders.Length * 2);
            }

            for (int i = 0; i < count; i++)
            {
                Collider hit = _colliders[i];
                if (hit == null)
                    continue;

                MagnetAttractable target = hit.GetComponentInParent<MagnetAttractable>();
                if (target != null && IsAimedAt(source, target.transform.position) && !results.Contains(target))
                    results.Add(target);
            }
        }

        public override bool IsAimedAt(Transform source, Vector3 originalTargetPosition)
        {
            Vector3 delta = originalTargetPosition - source.position;
            if (delta.sqrMagnitude > _range * _range)
                return false;

            if (delta.sqrMagnitude < 0.0001f)
                return true;

            return Vector3.Dot(source.forward, delta.normalized) >= Mathf.Cos(_fullAngle * 0.5f * Mathf.Deg2Rad);
        }
    }
}
