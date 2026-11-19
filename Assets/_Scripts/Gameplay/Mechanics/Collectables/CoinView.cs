
using DG.Tweening;
using Gameplay.Components.Animations;
using UnityEngine;

namespace Gameplay.Mechanics.Collectables
{
    public sealed class CoinView : MonoBehaviour
    {
        [SerializeField] private Coin _coin;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private RotationLoopAnimation _rotationAnimation;

        [Header("Collect Animation")]
        [SerializeField, Min(0.1f)] private float _duration = 0.45f;
        [SerializeField, Min(0f)] private float _riseHeight = 0.8f;
        [SerializeField, Min(1f)] private float _punchScale = 1.3f;
        [SerializeField] private float _rotationAngle = 360f;

        private Sequence _sequence;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private Vector3 _initialScale;
        private bool _isCollecting;

        private void Awake()
        {
            _initialPosition = _visualRoot.localPosition;
            _initialRotation = _visualRoot.localRotation;
            _initialScale = _visualRoot.localScale;
        }

        private void OnEnable()
        {
            ResetVisual();
            _rotationAnimation?.Restart();

            _coin.OnInteracted += OnInteracted;
        }

        private void OnDisable()
        {
            _coin.OnInteracted -= OnInteracted;

            _rotationAnimation?.Stop();

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

            _rotationAnimation?.Stop();

            float punchDuration = _duration * 0.25f;
            Vector3 currentRotation = _visualRoot.localEulerAngles;

            _sequence = DOTween.Sequence()
                .Append(_visualRoot.DOScale(_initialScale * _punchScale,punchDuration).SetEase(Ease.OutBack))
                .Append(_visualRoot.DOScale(Vector3.zero,_duration - punchDuration).SetEase(Ease.InBack))
                .Insert(0f,_visualRoot.DOLocalMoveY(_initialPosition.y + _riseHeight,_duration).SetEase(Ease.OutCubic))
                .Insert(0f,_visualRoot.DOLocalRotate(currentRotation + Vector3.up * _rotationAngle,_duration,RotateMode.FastBeyond360).SetEase(Ease.OutQuad))
                .OnComplete(() =>
                {
                    _sequence = null;
                    _coin.Hide();
                });
        }

        private void ResetVisual()
        {
            _isCollecting = false;

            _visualRoot.localPosition = _initialPosition;
            _visualRoot.localRotation = _initialRotation;
            _visualRoot.localScale = _initialScale;
        }
    }
}
