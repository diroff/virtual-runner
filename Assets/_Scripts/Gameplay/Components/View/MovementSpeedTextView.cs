using Gameplay.Components.Movement;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace Gameplay.Components.View
{
    public sealed class MovementSpeedTextView : MonoBehaviour
    {
        [Inject] private MovementSpeed _movementSpeed;
        [SerializeField] private TMP_Text _text;

        private int _displayedSpeed = int.MinValue;

        private void Awake()
        {
            if (_text == null)
                _text = GetComponentInChildren<TMP_Text>(true);
        }

        private void OnEnable()
        {
            if (_movementSpeed == null || _text == null)
            {
                Debug.LogError($"{nameof(MovementSpeedTextView)}: Movement speed or text is not assigned.", this);
                return;
            }

            _displayedSpeed = int.MinValue;
            _movementSpeed.OnSpeedChanged.AddListener(UpdateSpeed);
            UpdateSpeed(_movementSpeed.CurrentSpeed);
        }

        private void OnDisable()
        {
            if (_movementSpeed != null)
                _movementSpeed.OnSpeedChanged.RemoveListener(UpdateSpeed);
        }

        private void UpdateSpeed(float speed)
        {
            int displayed = Mathf.RoundToInt(speed * 10f);
            if (displayed == _displayedSpeed)
                return;

            _displayedSpeed = displayed;
            _text.SetText("{0:0.0}", displayed * 0.1f);
        }
    }
}
