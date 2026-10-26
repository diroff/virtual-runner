using Abstractions.Components.Movement;
using Cysharp.Threading.Tasks;
using Gameplay.Mechanics.LevelGeneration;
using Gameplay.Mechanics.Scoring;
using Gameplay.Models;
using UnityEngine;

namespace Tests
{
    public class TransformMovementDebug : MonoBehaviour
    {
        [SerializeField] private MovementBase _movement;
        [SerializeField] private LevelSegmentSpawner _levelSpawner;
        [SerializeField] private DistanceScoreTracker _distanceScoreTracker;

        private Score _score;

        private void Start()
        {
            if (_movement == null || _distanceScoreTracker == null)
            {
                Debug.LogError($"{nameof(TransformMovementDebug)}: Movement or DistanceScoreTracker is not assigned.", this);
                return;
            }

            _score = new Score();
            _score.OnValueChanged += HandleScoreChanged;

            _distanceScoreTracker.Initialize(_score);
            _distanceScoreTracker.OnMultiplierChanged += HandleMultiplierChanged;

            _levelSpawner.StartRunAsync().Forget();

            if (!_movement.StartMovement())
                return;

            _distanceScoreTracker.StartScoring();
        }

        private void OnDisable()
        {
            if (_distanceScoreTracker != null)
            {
                _distanceScoreTracker.StopScoring();
                _distanceScoreTracker.OnMultiplierChanged -= HandleMultiplierChanged;
            }

            if (_movement != null)
                _movement.StopMovement();

            if (_score != null)
                _score.OnValueChanged -= HandleScoreChanged;
        }

        private void HandleScoreChanged(long previousValue, long currentValue)
        {
            Debug.Log($"Score: {previousValue} -> {currentValue}");
        }

        private void HandleMultiplierChanged(float multiplier)
        {
            Debug.Log($"Score multiplier: x{multiplier}");
        }

        [ContextMenu("Add")]
        public void AddMult()
        {
            _distanceScoreTracker.AddMultiplier(1);
        }

        [ContextMenu("Remove")]
        public void RemoveMult()
        {
            _distanceScoreTracker.RemoveMultiplier(1);
        }
    }
}