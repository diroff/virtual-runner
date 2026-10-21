
using Gameplay.Components.Movement;
using UnityEngine;

namespace Tests
{
    public class TransformVelocityDebug : MonoBehaviour
    {
        [SerializeField] private TransformVelocity _velocityComponent;

        private void OnEnable()
        {
            if (_velocityComponent == null)
                return;

            _velocityComponent.OnSpeedUpdated.AddListener(LogSpeed);
            _velocityComponent.StartTracking();
        }

        private void OnDisable()
        {
            if (_velocityComponent == null)
                return;

            _velocityComponent.OnSpeedUpdated.RemoveListener(LogSpeed);
            _velocityComponent.StopTracking();
        }

        private void LogSpeed(float speed)
        {
            Debug.Log($"Speed: {speed:F2} m/s", this);
        }
    }
}
