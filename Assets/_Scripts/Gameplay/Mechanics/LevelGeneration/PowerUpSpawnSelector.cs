using UnityEngine;

namespace Gameplay.Mechanics.LevelGeneration
{
    public sealed class PowerUpSpawnSelector
    {
        private float _credit;

        public void Reset()
        {
            _credit = Random.value;
        }

        public bool Next(float probability)
        {
            probability = Mathf.Clamp01(probability);
            if (probability <= 0f)
            {
                _credit = 0f;
                return false;
            }

            _credit += probability;
            if (_credit < 1f)
                return false;

            _credit -= 1f;
            return true;
        }
    }
}
