using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Components.Movement
{
    public class MovementSpeed : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField, Min(0f)] private float _baseSpeed = 5f;

        public float BaseSpeed => _baseSpeed;
        public float CurrentSpeed { get; private set; }

        public UnityEvent<float> OnSpeedChanged = new();

        private readonly Dictionary<Object, float> _contributions = new();

        private void Awake()
        {
            RecalculateSpeed();
        }

        public void SetBaseSpeed(float speed)
        {
            _baseSpeed = Mathf.Max(0f, speed);
            RecalculateSpeed();
        }

        public void SetContribution(Object source, float value)
        {
            if (source == null)
            {
                Debug.LogError($"{nameof(MovementSpeed)}: Contribution source is null.", this);
                return;
            }

            _contributions[source] = value;
            RecalculateSpeed();
        }

        public void RemoveContribution(Object source)
        {
            if (source == null)
                return;

            if (_contributions.Remove(source))
                RecalculateSpeed();
        }

        private void RecalculateSpeed()
        {
            float speed = _baseSpeed;

            foreach (float contribution in _contributions.Values)
                speed += contribution;

            speed = Mathf.Max(0f, speed);

            if (Mathf.Approximately(CurrentSpeed, speed))
                return;

            CurrentSpeed = speed;
            OnSpeedChanged.Invoke(CurrentSpeed);
        }
    }
}