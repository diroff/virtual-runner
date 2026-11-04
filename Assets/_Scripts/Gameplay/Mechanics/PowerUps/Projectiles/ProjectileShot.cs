using Gameplay.Models;
using Services;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Projectiles
{
    public readonly struct ProjectileShot
    {
        public readonly GameObject Sender;
        public readonly Score Score;
        public readonly PoolService Pool;
        public readonly float Speed;
        public readonly float Range;
        public readonly float Damage;

        public ProjectileShot(GameObject sender, Score score, PoolService pool, float speed,
            float range, float damage)
        {
            Sender = sender;
            Score = score;
            Pool = pool;
            Speed = speed;
            Range = range;
            Damage = damage;
        }
    }
}
