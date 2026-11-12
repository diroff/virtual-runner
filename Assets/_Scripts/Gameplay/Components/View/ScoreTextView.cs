using Gameplay.Mechanics.Scoring;
using Gameplay.Models;
using TMPro;
using UnityEngine;

namespace Gameplay.Components.View
{
    public sealed class ScoreTextView : MonoBehaviour
    {
        [SerializeField] private DistanceScoreTracker _scoreTracker;
        [SerializeField] private TMP_Text _text;

        private Score _score;

        private void Awake()
        {
            if (_text == null)
                _text = GetComponentInChildren<TMP_Text>(true);
        }

        private void OnEnable()
        {
            if (_scoreTracker == null || _text == null)
            {
                Debug.LogError($"{nameof(ScoreTextView)}: Score tracker or text is not assigned.", this);
                return;
            }

            _scoreTracker.OnScoreInitialized += Bind;
            Bind(_scoreTracker.Score);
        }

        private void OnDisable()
        {
            if (_scoreTracker != null)
                _scoreTracker.OnScoreInitialized -= Bind;

            Unbind();
        }

        private void Bind(Score score)
        {
            if (!ReferenceEquals(_score, score))
            {
                Unbind();
                _score = score;

                if (_score != null)
                    _score.OnValueChanged += HandleValueChanged;
            }

            UpdateText(_score?.Value ?? 0);
        }

        private void Unbind()
        {
            if (_score != null)
                _score.OnValueChanged -= HandleValueChanged;

            _score = null;
        }

        private void HandleValueChanged(long previousValue, long currentValue)
        {
            UpdateText(currentValue);
        }

        private void UpdateText(long value)
        {
            if (_text != null)
                _text.text = value.ToString();
        }
    }
}
