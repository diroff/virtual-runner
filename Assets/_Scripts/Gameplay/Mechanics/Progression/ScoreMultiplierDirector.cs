using Gameplay.Mechanics.Scoring;
using UnityEngine;

namespace Gameplay.Mechanics.Progression
{
    public sealed class ScoreMultiplierDirector : DistanceValueDirectorBase
    {
        [SerializeField] private DistanceScoreTracker _scoreTracker;

        private float _initialMultiplier;

        protected override bool OnStart()
        {
            if (_scoreTracker == null)
            {
                Debug.LogError($"{nameof(ScoreMultiplierDirector)}: DistanceScoreTracker is not assigned.", this);
                return false;
            }

            _initialMultiplier = _scoreTracker.Multiplier;
            return true;
        }

        protected override void ApplyValue(float value)
        {
            _scoreTracker.SetMultiplier(Mathf.Max(0.01f, value));
        }

        protected override void OnStop()
        {
            if (_scoreTracker != null)
                _scoreTracker.SetMultiplier(_initialMultiplier);
        }
    }
}
