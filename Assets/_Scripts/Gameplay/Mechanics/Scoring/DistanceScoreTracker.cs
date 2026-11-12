using System;
using System.Collections.Generic;
using Gameplay.Mechanics.Progression;
using Gameplay.Models;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Mechanics.Scoring
{
    public class DistanceScoreTracker : MonoBehaviour
    {
        [Inject] private MovementDistanceTracker _distanceTracker;
        [SerializeField, Min(0f)] private float _pointsPerMeter = 1f;
        [SerializeField, Min(0.01f)] private float _initialMultiplier = 1f;

        private readonly Dictionary<UnityEngine.Object, float> _multiplierContributions = new();
        private Score _score;
        private double _fractionalPoints;
        private float _baseMultiplier;
        private float _multiplier;
        private bool _isScoring;

        public bool IsScoring => _isScoring;
        public float BaseMultiplier => _baseMultiplier;
        public float Multiplier => _multiplier;
        public Score Score => _score;

        public event Action<Score> OnScoreInitialized;
        public event Action OnScoringStarted;
        public event Action OnScoringStopped;
        public event Action<float> OnMultiplierChanged;
        public event Action<long> OnPointsAwarded;

        private void Awake()
        {
            _baseMultiplier = Mathf.Max(0.01f, _initialMultiplier);
            _multiplier = _baseMultiplier;
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
            _multiplierContributions.Clear();
            SetMultiplier(_initialMultiplier);
            RecalculateMultiplier();
            OnScoreInitialized?.Invoke(_score);
        }

        public bool StartScoring()
        {
            if (_score == null || _distanceTracker == null)
                return false;

            if (_isScoring)
                return true;

            _distanceTracker.OnDistanceAdvanced += HandleDistanceAdvanced;
            _distanceTracker.OnTrackingStopped += StopScoring;
            _isScoring = true;
            OnScoringStarted?.Invoke();
            return true;
        }

        public void StopScoring()
        {
            if (!_isScoring)
                return;

            if (_distanceTracker != null)
            {
                _distanceTracker.OnDistanceAdvanced -= HandleDistanceAdvanced;
                _distanceTracker.OnTrackingStopped -= StopScoring;
            }

            _isScoring = false;
            OnScoringStopped?.Invoke();
        }

        public void SetMultiplier(float multiplier)
        {
            if (float.IsNaN(multiplier) || float.IsInfinity(multiplier) || multiplier <= 0f)
                return;

            _baseMultiplier = multiplier;
            RecalculateMultiplier();
        }

        public void AddMultiplier(float amount)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f)
                return;

            SetMultiplier(_baseMultiplier + amount);
        }

        public void RemoveMultiplier(float amount)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f)
                return;

            SetMultiplier(_baseMultiplier - amount);
        }

        public void SetMultiplierContribution(UnityEngine.Object source, float value)
        {
            if (source == null || float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                return;

            _multiplierContributions[source] = value;
            RecalculateMultiplier();
        }

        public void RemoveMultiplierContribution(UnityEngine.Object source)
        {
            if (source != null && _multiplierContributions.Remove(source))
                RecalculateMultiplier();
        }

        public void ResetScoring()
        {
            StopScoring();
            _fractionalPoints = 0d;
            _score?.Reset();
            _multiplierContributions.Clear();
            SetMultiplier(_initialMultiplier);
            RecalculateMultiplier();
        }

        private void RecalculateMultiplier()
        {
            float result = _baseMultiplier;
            foreach (float contribution in _multiplierContributions.Values)
                result *= contribution;

            if (Mathf.Approximately(_multiplier, result))
                return;

            _multiplier = result;
            OnMultiplierChanged?.Invoke(result);
        }

        private void HandleDistanceAdvanced(float distance)
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
