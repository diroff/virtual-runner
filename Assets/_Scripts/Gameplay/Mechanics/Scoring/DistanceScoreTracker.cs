using System;
using Abstractions.Components.Movement;
using Gameplay.Models;
using UnityEngine;

namespace Gameplay.Mechanics.Scoring
{
    public class DistanceScoreTracker : MonoBehaviour
    {
        [SerializeField] private MovementBase _movement;
        [SerializeField, Min(0f)] private float _pointsPerMeter = 1f;
        [SerializeField, Min(0.01f)] private float _initialMultiplier = 1f;

        private Score _score;
        private double _fractionalPoints;
        private float _multiplier;
        private bool _isScoring;

        public bool IsScoring => _isScoring;
        public float Multiplier => _multiplier;
        public Score Score => _score;

        public event Action OnScoringStarted;
        public event Action OnScoringStopped;
        public event Action<float> OnMultiplierChanged;
        public event Action<long> OnPointsAwarded;

        private void Awake()
        {
            _multiplier = Mathf.Max(0.01f, _initialMultiplier);
        }

        private void OnDisable()
        {
            StopScoring();
        }

        public void Initialize(Score score)
        {
            if (score == null)
                throw new ArgumentNullException(nameof(score));

            StopScoring();

            _score = score;
            _fractionalPoints = 0d;

            SetMultiplier(_initialMultiplier);
        }

        public bool StartScoring()
        {
            if (_score == null || _movement == null)
                return false;

            if (_isScoring)
                return true;

            _movement.OnDistanceMoved += HandleDistanceMoved;
            _isScoring = true;

            OnScoringStarted?.Invoke();

            return true;
        }

        public void StopScoring()
        {
            if (!_isScoring)
                return;

            if (_movement != null)
                _movement.OnDistanceMoved -= HandleDistanceMoved;

            _isScoring = false;

            OnScoringStopped?.Invoke();
        }

        public void SetMultiplier(float multiplier)
        {
            if (float.IsNaN(multiplier) || float.IsInfinity(multiplier) || multiplier <= 0f)
                return;

            if (Mathf.Approximately(_multiplier, multiplier))
                return;

            _multiplier = multiplier;
            OnMultiplierChanged?.Invoke(_multiplier);
        }

        public void AddMultiplier(float amount)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f)
                return;

            SetMultiplier(_multiplier + amount);
        }

        public void RemoveMultiplier(float amount)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f)
                return;

            float result = _multiplier - amount;

            if (result <= 0f)
                return;

            SetMultiplier(result);
        }

        public void ResetScoring()
        {
            StopScoring();

            _fractionalPoints = 0d;
            _score?.Reset();

            SetMultiplier(_initialMultiplier);
        }

        private void HandleDistanceMoved(float distance)
        {
            if (!_isScoring || distance <= 0f || _pointsPerMeter <= 0f)
                return;

            double totalPoints = _fractionalPoints + distance * _pointsPerMeter * _multiplier;
            long earnedPoints = (long)Math.Floor(totalPoints);

            _fractionalPoints = totalPoints - earnedPoints;

            if (earnedPoints <= 0)
                return;

            _score.Add(earnedPoints);
            OnPointsAwarded?.Invoke(earnedPoints);
        }
    }
}