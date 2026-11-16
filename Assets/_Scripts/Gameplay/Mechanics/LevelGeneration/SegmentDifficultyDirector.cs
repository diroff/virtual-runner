using Data.LevelGeneration;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Mechanics.LevelGeneration
{
    public sealed class SegmentDifficultyDirector : MonoBehaviour
    {
        [Inject] private LevelSegmentSpawner _spawner;
        [SerializeField] private SegmentDifficultyProfile _profile;

        private readonly WeightedDifficultySelector _selector = new();

        private void OnEnable()
        {
            if (_spawner == null)
                return;

            _spawner.OnLevelPrepared += ResetSelection;
            _spawner.OnSegmentRequested += HandleSegmentRequested;
            ResetSelection();
        }

        private void OnDisable()
        {
            if (_spawner == null)
                return;

            _spawner.OnLevelPrepared -= ResetSelection;
            _spawner.OnSegmentRequested -= HandleSegmentRequested;
        }

        private void ResetSelection()
        {
            _selector.Reset();
        }

        private void HandleSegmentRequested(float startDistance)
        {
            if (_profile == null)
                return;

            _spawner.SetDifficulty(_selector.Next(_profile.Evaluate(startDistance)));
        }
    }
}
