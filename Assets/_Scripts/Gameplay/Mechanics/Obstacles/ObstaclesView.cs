using DG.Tweening;
using UnityEngine;

namespace Gameplay.Mechanics.Obstacles
{
    public sealed class ObstaclesView : MonoBehaviour
    {
        [SerializeField] private ObstacleBase _obstacle;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private ParticleSystem _destroyParticles;

        [SerializeField, Min(0.01f)] private float _impactDuration = 0.13f;
        [SerializeField, Min(1f)] private float _impactScale = 1.12f;
        [SerializeField, Min(0f)] private float _impactWobble = 12f;
        [SerializeField, Min(0.01f)] private float _disappearDuration = 0.28f;
        [SerializeField, Min(0f)] private float _riseDistance = 0.3f;
        [SerializeField] private float _spinAngle = 45f;
        [SerializeField] private Ease _impactEase = Ease.OutBack;
        [SerializeField] private Ease _disappearEase = Ease.InBack;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private Vector3 _initialScale;
        private Sequence _sequence;

        private void Awake()
        {
            if (_obstacle == null)
                _obstacle = GetComponentInParent<ObstacleBase>();

            if (_visualRoot == null)
            {
                Debug.LogError($"{nameof(ObstaclesView)}: Visual Root is not assigned.", this);
                return;
            }

            _initialPosition = _visualRoot.localPosition;
            _initialRotation = _visualRoot.localRotation;
            _initialScale = _visualRoot.localScale;
        }

        private void OnEnable()
        {
            if (_obstacle == null)
                return;

            _obstacle.RegisterDeathPresentation(PlayDeath);
            _obstacle.OnRespawned += ResetVisual;
        }

        private void OnDisable()
        {
            if (_obstacle != null)
            {
                _obstacle.UnregisterDeathPresentation(PlayDeath);
                _obstacle.OnRespawned -= ResetVisual;
            }

            ResetVisual();
        }

        private void PlayDeath()
        {
            if (_obstacle == null)
                return;

            if (!isActiveAndEnabled || _visualRoot == null)
            {
                _obstacle.Hide();
                return;
            }

            if (_sequence != null && _sequence.IsActive())
                return;

            _destroyParticles?.Play();

            _sequence = DOTween.Sequence()
                .Append(_visualRoot.DOScale(_initialScale * _impactScale, _impactDuration).SetEase(_impactEase))
                .Join(_visualRoot.DOPunchRotation(new Vector3(0f, 0f, _impactWobble), _impactDuration, 6, 0.5f))
                .Append(_visualRoot.DOScale(Vector3.zero, _disappearDuration).SetEase(_disappearEase))
                .Join(_visualRoot.DOLocalMove(_initialPosition + Vector3.up * _riseDistance, _disappearDuration)
                    .SetEase(Ease.OutQuad))
                .Join(_visualRoot.DOLocalRotateQuaternion(_initialRotation * Quaternion.Euler(0f, _spinAngle, 0f),
                    _disappearDuration).SetEase(Ease.InQuad))
                .OnComplete(() => _obstacle.Hide());
        }

        private void ResetVisual()
        {
            _sequence?.Kill();
            _sequence = null;

            if (_visualRoot != null)
            {
                _visualRoot.localPosition = _initialPosition;
                _visualRoot.localRotation = _initialRotation;
                _visualRoot.localScale = _initialScale;
            }

            if (_destroyParticles != null)
                _destroyParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
