using System;
using UnityEngine;

namespace Data.PowerUps
{
    [Serializable]
    public struct PowerUpLevelSettings
    {
        [Min(0f)] public float Duration;
        [Min(0)] public int Charges;
        public long UpgradeCost;
    }
}
