using System;
using UnityEngine;

namespace Data.PowerUps
{
    [CreateAssetMenu(menuName = "Data/Power Ups/Power Up")]
    public sealed class PowerUpDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private PowerUpSlot _slot;
        [SerializeField] private PowerUpLevelSettings[] _levels = Array.Empty<PowerUpLevelSettings>();

        public string Id => _id;
        public PowerUpSlot Slot => _slot;
        public int MaxLevel => _levels?.Length ?? 0;

        public PowerUpLevelSettings GetSettings(int level)
        {
            if (MaxLevel == 0)
                throw new InvalidOperationException($"{name}: Level settings are missing.");

            return _levels[Mathf.Clamp(level, 1, MaxLevel) - 1];
        }
    }
}
