using Gameplay.Mechanics.Progression;
using TMPro;
using UnityEngine;

namespace Gameplay.Components.View
{
    public sealed class MovementDistanceTextView : MonoBehaviour
    {
        [SerializeField] private MovementDistanceTracker _distanceTracker;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private string _suffix = " м";

        private int _displayedMeters = -1;

        private void Awake()
        {
            if (_text == null)
                _text = GetComponentInChildren<TMP_Text>(true);
        }

        private void OnEnable()
        {
            if (_distanceTracker == null || _text == null)
            {
                Debug.LogError($"{nameof(MovementDistanceTextView)}: Distance tracker or text is not assigned.", this);
                return;
            }

            _displayedMeters = -1;
            _distanceTracker.OnDistanceChanged += UpdateDistance;
            UpdateDistance(_distanceTracker.Distance);
        }

        private void OnDisable()
        {
            if (_distanceTracker != null)
                _distanceTracker.OnDistanceChanged -= UpdateDistance;
        }

        private void UpdateDistance(float distance)
        {
            int meters = Mathf.FloorToInt(Mathf.Max(0f, distance));
            if (meters == _displayedMeters)
                return;

            _displayedMeters = meters;
            _text.text = $"{meters}{_suffix}";
        }
    }
}
