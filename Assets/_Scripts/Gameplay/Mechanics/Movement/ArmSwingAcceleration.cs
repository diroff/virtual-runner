using Gameplay.Components.Movement;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Mechanics.Movement
{
    /// <summary>
    /// Adds speed only when both controllers perform alternating, opposite vertical strokes.
    /// Requires several valid strokes to establish a running rhythm; unrelated hand movement
    /// does not contribute. TransformVelocity continues to serve other features independently.
    /// </summary>
    public class ArmSwingAcceleration : MonoBehaviour
    {
        [Header("Hand Tracking")]
        [SerializeField] private TransformVelocity _leftHandVelocity;
        [SerializeField] private TransformVelocity _rightHandVelocity;

        [Header("Speed")]
        [SerializeField] private MovementSpeed _movementSpeed;
        // Existing serialized field names are intentionally preserved for old scenes/prefabs.
        [SerializeField, Min(0f)] private float _minimumHandSpeed = 0.5f;
        [SerializeField, Min(0.01f)] private float _maximumHandSpeed = 2.5f;
        [SerializeField, Min(0f)] private float _maximumAdditionalSpeed = 4f;

        [Header("Alternating Running Pattern")]
        [Tooltip("Both hands must move vertically in opposite directions at least this fast (m/s).")]
        [SerializeField, Min(0.01f)] private float _minimumVerticalSpeed = 0.25f;
        [Tooltip("Minimum distance travelled by EACH hand during a valid opposed stroke (meters).")]
        [SerializeField, Min(0.01f)] private float _minimumStrokeDistance = 0.07f;
        [Tooltip("Prevents controller jitter from being interpreted as a stroke (seconds).")]
        [SerializeField, Min(0.01f)] private float _minimumStrokeDuration = 0.08f;
        [Tooltip("Number of valid changes in stroke direction before running starts.")]
        [SerializeField, Range(1, 4)] private int _requiredAlternations = 2;
        [Tooltip("Minimum time between registered opposed strokes (seconds).")]
        [SerializeField, Min(0.01f)] private float _minimumHalfCycleTime = 0.12f;
        [Tooltip("Maximum time between registered opposed strokes (seconds).")]
        [SerializeField, Min(0.02f)] private float _maximumHalfCycleTime = 0.85f;
        [Tooltip("Allows a brief pause as hands reverse direction (seconds).")]
        [SerializeField, Min(0f)] private float _directionChangeGraceTime = 0.15f;

        [Header("Smoothing")]
        [Tooltip("Reduces jitter in vertical controller velocity (seconds).")]
        [SerializeField, Min(0f)] private float _velocitySmoothingTime = 0.05f;
        [Tooltip("Time constant for increasing the running bonus (seconds).")]
        [SerializeField, Min(0f)] private float _accelerationSmoothingTime = 0.12f;
        [Tooltip("Time constant for losing the running bonus (seconds).")]
        [SerializeField, Min(0f)] private float _decelerationSmoothingTime = 0.20f;

        // Public API preserved for other systems / persistent UnityEvent listeners.
        public float CurrentAdditionalSpeed { get; private set; }
        public UnityEvent<float> OnAdditionalSpeedChanged = new();

        // Optional diagnostic API; independent of the MovementSpeed contribution.
        public bool IsRunning { get; private set; }
        public int ConsecutiveAlternations => _consecutiveAlternations;
        public float PairedVerticalSpeed { get; private set; }

        private bool _leftTrackingAcquired;
        private bool _rightTrackingAcquired;
        private float _smoothedLeftVerticalVelocity;
        private float _smoothedRightVerticalVelocity;
        private float _clock;
        private float _lastOpposedMotionTime = float.NegativeInfinity;
        private float _lastAcceptedStrokeTime = float.NegativeInfinity;
        private int _strokePhase; // +1: left up/right down, -1: left down/right up.
        private int _lastAcceptedPhase;
        private int _consecutiveAlternations;
        private bool _currentStrokeAccepted;
        private float _currentStrokeDistance;
        private float _currentStrokeDuration;
        private float _lastPairedVerticalSpeed;

        private void OnEnable()
        {
            ResetDetection();

            if (_movementSpeed == null)
            {
                Debug.LogError($"{nameof(ArmSwingAcceleration)}: MovementSpeed is not assigned.", this);
                return;
            }

            if (_leftHandVelocity == null || _rightHandVelocity == null ||
                _leftHandVelocity == _rightHandVelocity)
            {
                Debug.LogError($"{nameof(ArmSwingAcceleration)}: Two different hand velocity trackers are required.", this);
                return;
            }

            _leftTrackingAcquired = _leftHandVelocity.AcquireTracking(this);
            _rightTrackingAcquired = _rightHandVelocity.AcquireTracking(this);

            // Do not keep one tracker running if the other cannot be acquired.
            if (!_leftTrackingAcquired || !_rightTrackingAcquired)
            {
                if (_leftTrackingAcquired)
                    _leftHandVelocity.ReleaseTracking(this);
                if (_rightTrackingAcquired)
                    _rightHandVelocity.ReleaseTracking(this);

                _leftTrackingAcquired = false;
                _rightTrackingAcquired = false;
            }
        }

        private void OnDisable()
        {
            if (_leftTrackingAcquired && _leftHandVelocity != null)
                _leftHandVelocity.ReleaseTracking(this);

            if (_rightTrackingAcquired && _rightHandVelocity != null)
                _rightHandVelocity.ReleaseTracking(this);

            _leftTrackingAcquired = false;
            _rightTrackingAcquired = false;

            if (_movementSpeed != null)
                _movementSpeed.RemoveContribution(this);

            CurrentAdditionalSpeed = 0f;
            ResetDetection();
        }

        // Velocity trackers sample in Update. Reading both in LateUpdate prevents
        // order-dependent pairs of samples from different frames.
        private void LateUpdate()
        {
            if (_movementSpeed == null)
                return;

            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
                return;

            _clock += deltaTime;

            bool ready = _leftTrackingAcquired && _rightTrackingAcquired &&
                         _leftHandVelocity != null && _rightHandVelocity != null &&
                         _leftHandVelocity.isActiveAndEnabled && _rightHandVelocity.isActiveAndEnabled &&
                         _leftHandVelocity.IsTracking && _rightHandVelocity.IsTracking;

            if (!ready)
            {
                ResetPattern();
                UpdateAdditionalSpeed(0f, deltaTime);
                return;
            }

            float leftVelocity = _leftHandVelocity.VelocityInWorldAxes.y;
            float rightVelocity = _rightHandVelocity.VelocityInWorldAxes.y;
            float velocityWeight = ResponseWeight(deltaTime, _velocitySmoothingTime);

            _smoothedLeftVerticalVelocity = Mathf.Lerp(_smoothedLeftVerticalVelocity, leftVelocity, velocityWeight);
            _smoothedRightVerticalVelocity = Mathf.Lerp(_smoothedRightVerticalVelocity, rightVelocity, velocityWeight);

            int phase = GetOpposedPhase(_smoothedLeftVerticalVelocity, _smoothedRightVerticalVelocity);
            PairedVerticalSpeed = phase == 0 ? 0f :
                Mathf.Min(Mathf.Abs(_smoothedLeftVerticalVelocity), Mathf.Abs(_smoothedRightVerticalVelocity));

            if (phase != 0)
            {
                _lastOpposedMotionTime = _clock;
                _lastPairedVerticalSpeed = PairedVerticalSpeed;
                ProcessOpposedStroke(phase, PairedVerticalSpeed, deltaTime);
            }
            else if (_clock - _lastOpposedMotionTime > _directionChangeGraceTime)
            {
                ResetCurrentStroke();
            }

            // A long gap breaks cadence, even if the controllers are still moving.
            if (_clock - _lastAcceptedStrokeTime > _maximumHalfCycleTime)
                ClearCadence();

            IsRunning = _consecutiveAlternations >= _requiredAlternations &&
                        _clock - _lastOpposedMotionTime <= _directionChangeGraceTime &&
                        _clock - _lastAcceptedStrokeTime <= _maximumHalfCycleTime;

            float effectiveSpeed = phase != 0 ? PairedVerticalSpeed : _lastPairedVerticalSpeed;
            float targetSpeed = IsRunning
                ? Mathf.InverseLerp(_minimumHandSpeed, _maximumHandSpeed, effectiveSpeed) * _maximumAdditionalSpeed
                : 0f;

            UpdateAdditionalSpeed(targetSpeed, deltaTime);
        }

        private int GetOpposedPhase(float leftVertical, float rightVertical)
        {
            if (leftVertical >= _minimumVerticalSpeed && rightVertical <= -_minimumVerticalSpeed)
                return 1;

            if (leftVertical <= -_minimumVerticalSpeed && rightVertical >= _minimumVerticalSpeed)
                return -1;

            return 0;
        }

        private void ProcessOpposedStroke(int phase, float pairedSpeed, float deltaTime)
        {
            if (phase != _strokePhase)
            {
                _strokePhase = phase;
                _currentStrokeDistance = 0f;
                _currentStrokeDuration = 0f;
                _currentStrokeAccepted = false;
            }

            if (_currentStrokeAccepted)
                return;

            _currentStrokeDistance += pairedSpeed * deltaTime;
            _currentStrokeDuration += deltaTime;

            if (_currentStrokeDistance < _minimumStrokeDistance ||
                _currentStrokeDuration < _minimumStrokeDuration)
                return;

            _currentStrokeAccepted = true;

            float interval = _clock - _lastAcceptedStrokeTime;
            bool alternating = _lastAcceptedPhase == -phase &&
                               interval >= _minimumHalfCycleTime &&
                               interval <= _maximumHalfCycleTime;

            _consecutiveAlternations = alternating ?
                Mathf.Min(_requiredAlternations, _consecutiveAlternations + 1) : 0;

            _lastAcceptedPhase = phase;
            _lastAcceptedStrokeTime = _clock;
        }

        private void UpdateAdditionalSpeed(float target, float deltaTime)
        {
            float smoothing = target > CurrentAdditionalSpeed
                ? _accelerationSmoothingTime
                : _decelerationSmoothingTime;

            float nextSpeed = Mathf.Lerp(CurrentAdditionalSpeed, target, ResponseWeight(deltaTime, smoothing));

            // Avoid a never-ending tail of tiny contributions from exponential smoothing.
            if (target <= 0f && nextSpeed < 0.01f)
                nextSpeed = 0f;

            if (Mathf.Approximately(CurrentAdditionalSpeed, nextSpeed))
                return;

            CurrentAdditionalSpeed = nextSpeed;
            _movementSpeed.SetContribution(this, nextSpeed);
            OnAdditionalSpeedChanged.Invoke(nextSpeed);
        }

        private static float ResponseWeight(float deltaTime, float timeConstant)
        {
            return timeConstant <= 0f ? 1f : 1f - Mathf.Exp(-deltaTime / timeConstant);
        }

        private void ResetCurrentStroke()
        {
            _strokePhase = 0;
            _currentStrokeAccepted = false;
            _currentStrokeDistance = 0f;
            _currentStrokeDuration = 0f;
        }

        private void ClearCadence()
        {
            _lastAcceptedPhase = 0;
            _lastAcceptedStrokeTime = float.NegativeInfinity;
            _consecutiveAlternations = 0;
        }

        private void ResetPattern()
        {
            ResetCurrentStroke();
            ClearCadence();
            _smoothedLeftVerticalVelocity = 0f;
            _smoothedRightVerticalVelocity = 0f;
            _lastOpposedMotionTime = float.NegativeInfinity;
            _lastPairedVerticalSpeed = 0f;
            PairedVerticalSpeed = 0f;
            IsRunning = false;
        }

        private void ResetDetection()
        {
            _clock = 0f;
            ResetPattern();
        }

        private void OnValidate()
        {
            _minimumHandSpeed = Mathf.Max(0f, _minimumHandSpeed);
            _maximumHandSpeed = Mathf.Max(_minimumHandSpeed + 0.01f, _maximumHandSpeed);
            _maximumAdditionalSpeed = Mathf.Max(0f, _maximumAdditionalSpeed);
            _minimumVerticalSpeed = Mathf.Max(0.01f, _minimumVerticalSpeed);
            _minimumStrokeDistance = Mathf.Max(0.01f, _minimumStrokeDistance);
            _minimumStrokeDuration = Mathf.Max(0.01f, _minimumStrokeDuration);
            _requiredAlternations = Mathf.Clamp(_requiredAlternations, 1, 4);
            _minimumHalfCycleTime = Mathf.Max(0.01f, _minimumHalfCycleTime);
            _maximumHalfCycleTime = Mathf.Max(_minimumHalfCycleTime + 0.01f, _maximumHalfCycleTime);
            _directionChangeGraceTime = Mathf.Max(0f, _directionChangeGraceTime);
            _velocitySmoothingTime = Mathf.Max(0f, _velocitySmoothingTime);
            _accelerationSmoothingTime = Mathf.Max(0f, _accelerationSmoothingTime);
            _decelerationSmoothingTime = Mathf.Max(0f, _decelerationSmoothingTime);
        }
    }
}
