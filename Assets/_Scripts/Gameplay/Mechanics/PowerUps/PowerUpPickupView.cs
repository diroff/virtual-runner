using DG.Tweening;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps
{
    public class PowerUpPickupView : MonoBehaviour
    {
        [SerializeField] private PowerUpPickup _powerUp;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private Transform _visualRoot;

        [Header("Collect Animation")]
        [SerializeField, Min(0.1f)] private float _duration = 0.5f;
        [SerializeField] private float _riseHeight = 1.2f;
        [SerializeField, Min(1f)] private float _punchScale = 1.15f;

        private Sequence _sequence;
        private Vector3 _initialPosition;
        private Vector3 _initialScale;
        private bool _isCollecting;

        private void Awake()
        {
            _initialPosition = _visualRoot.localPosition;
            _initialScale = _visualRoot.localScale;
        }

        private void OnEnable()
        {
            ResetVisual();
            _powerUp.OnInteracted += OnInteracted;
        }

        private void OnDisable()
        {
            _powerUp.OnInteracted -= OnInteracted;
            _sequence?.Kill();
            _sequence = null;
            ResetVisual();
        }

        private void OnInteracted(GameObject interactor)
        {
            if (_isCollecting)
                return;

            PlayCollectAnimation();
        }

        private void PlayCollectAnimation()
        {
            _isCollecting = true;
            _particleSystem?.Play();

            _sequence = DOTween.Sequence()
                .Append(_visualRoot.DOScale(_initialScale * _punchScale, 0.12f).SetEase(Ease.OutQuad))
                .Append(_visualRoot.DOScale(Vector3.zero, _duration - 0.12f).SetEase(Ease.InBack))
                .Join(_visualRoot.DOLocalMoveY(_initialPosition.y + _riseHeight,_duration - 0.12f).SetEase(Ease.OutCubic))
                .OnComplete(() =>
                {
                    _sequence = null;
                    _powerUp.Hide();
                });
        }

        private void ResetVisual()
        {
            _isCollecting = false;
            _visualRoot.localPosition = _initialPosition;
            _visualRoot.localScale = _initialScale;
        }
    }
}
