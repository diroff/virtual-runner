using Data.LevelGeneration;
using UnityEngine;

namespace Gameplay.Mechanics.LevelGeneration
{
    public sealed class WeightedDifficultySelector
    {
        private readonly float[] _credits = new float[3];

        public void Reset()
        {
            for (int i = 0; i < _credits.Length; i++)
                _credits[i] = 0f;
        }

        public SegmentDifficulty Next(SegmentDifficultyProfile.DifficultyWeights weights)
        {
            float sum = weights.Easy + weights.Medium + weights.Hard;
            if (sum <= 0f)
            {
                Reset();
                return SegmentDifficulty.Easy;
            }

            float inverseSum = 1f / sum;
            float easy = weights.Easy * inverseSum;
            float medium = weights.Medium * inverseSum;
            float hard = weights.Hard * inverseSum;

            _credits[0] = easy > 0f ? _credits[0] + easy : 0f;
            _credits[1] = medium > 0f ? _credits[1] + medium : 0f;
            _credits[2] = hard > 0f ? _credits[2] + hard : 0f;

            int selected = -1;
            float bestScore = float.NegativeInfinity;

            for (int i = 0; i < _credits.Length; i++)
            {
                float probability = i == 0 ? easy : i == 1 ? medium : hard;
                if (probability <= 0f)
                    continue;

                float score = _credits[i] + Random.value * 0.05f;
                if (score <= bestScore)
                    continue;

                bestScore = score;
                selected = i;
            }

            _credits[selected] -= 1f;
            return (SegmentDifficulty)selected;
        }
    }
}
