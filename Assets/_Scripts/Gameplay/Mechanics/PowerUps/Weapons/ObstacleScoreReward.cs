using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Weapons
{
    public sealed class ObstacleScoreReward : MonoBehaviour
    {
        [SerializeField] private long _points = 10;
        public long Points => System.Math.Max(0, _points);
    }
}
