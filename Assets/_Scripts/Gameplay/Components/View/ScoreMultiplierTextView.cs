using Gameplay.Mechanics.Scoring;
using Gameplay.Models;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace Gameplay.Components.View
{
    public sealed class ScoreMultiplierTextView : MonoBehaviour
    {
        [Inject] private DistanceScoreTracker _scoreTracker;
        [SerializeField] private TMP_Text _text;

        private int _displayedMultiplier = int.MinValue;

        private void Awake()
        {
            if (_text == null)
                _text = GetComponentInChildren<TMP_Text>(true);
        }

        private void OnEnable()
        {
            if (_scoreTracker == null || _text == null)
            {
                Debug.LogError($"{nameof(ScoreMultiplierTextView)}: Score tracker or text is not assigned.", this);
                return;
            }

            _displayedMultiplier = int.MinValue;
            _scoreTracker.OnMultiplierChanged += UpdateMultiplier;
            _scoreTracker.OnScoreInitialized += HandleScoreInitialized;
            UpdateMultiplier(_scoreTracker.Multiplier);
        }

        private void OnDisable()
        {
            if (_scoreTracker != null)
            {
                _scoreTracker.OnMultiplierChanged -= UpdateMultiplier;
                _scoreTracker.OnScoreInitialized -= HandleScoreInitialized;
            }
        }

        private void HandleScoreInitialized(Score score)
        {
            UpdateMultiplier(_scoreTracker.Multiplier);
        }

        private void UpdateMultiplier(float multiplier)
        {
            int displayed = Mathf.RoundToInt(multiplier);
            if (displayed == _displayedMultiplier)
                return;

            _displayedMultiplier = displayed;
            _text.text = $"x{displayed}";
        }
    }
}
