using System;
using System.Collections.Generic;
using Data.PowerUps;

namespace Gameplay.Models
{
    public sealed class PowerUpProgression
    {
        private readonly Dictionary<string, int> _levels;

        public event Action<string, int> OnLevelChanged;

        public PowerUpProgression(Dictionary<string, int> levels)
        {
            _levels = levels ?? throw new ArgumentNullException(nameof(levels));
        }

        public int GetLevel(PowerUpDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                return 1;

            return _levels.TryGetValue(definition.Id, out int level)
                ? Math.Max(1, Math.Min(level, Math.Max(1, definition.MaxLevel)))
                : 1;
        }

        public bool TrySetLevel(PowerUpDefinition definition, int level)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id) ||
                level < 1 || level > definition.MaxLevel)
                return false;

            int previousLevel = GetLevel(definition);
            if (previousLevel == level)
                return false;

            _levels[definition.Id] = level;
            OnLevelChanged?.Invoke(definition.Id, level);
            return true;
        }
    }
}
