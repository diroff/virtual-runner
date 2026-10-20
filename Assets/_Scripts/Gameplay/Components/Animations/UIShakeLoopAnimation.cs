using DG.Tweening;
using UnityEngine;

namespace Gameplay.Components.Animations
{
    public sealed class UIShakeLoopAnimation : TweenAnimationBase
    {
        [SerializeField] private RectTransform _rectTarget;

        [SerializeField] private Vector2 _offset = new(20f, 20f);
        [SerializeField] private Vector3 _pulseScale = Vector3.one * 1.1f;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private Ease _ease = Ease.InOutSine;

        private Vector2 _initialPosition;
        private Vector3 _initialScale;

        protected override void Reset()
        {
            _rectTarget = GetComponent<RectTransform>();
            _target = _rectTarget;
        }

        protected override void Awake()
        {
            if (_rectTarget == null)
                _rectTarget = GetComponent<RectTransform>();

            _target = _rectTarget;

            base.Awake();
        }

        protected override void CaptureState()
        {
            _initialPosition = _rectTarget.anchoredPosition;
            _initialScale = _rectTarget.localScale;
        }

        protected override void RestoreState()
        {
            _rectTarget.anchoredPosition = _initialPosition;
            _rectTarget.localScale = _initialScale;
        }

        protected override Tween BuildTween()
        {
            return DOTween.Sequence()
                .Append(_rectTarget.DOAnchorPos(_initialPosition + _offset, _duration).SetEase(_ease))
                .Join(_rectTarget.DOScale(_pulseScale, _duration).SetEase(_ease))
                .Append(_rectTarget.DOAnchorPos(_initialPosition - _offset, _duration).SetEase(_ease))
                .Join(_rectTarget.DOScale(_initialScale, _duration).SetEase(_ease))
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void SetOffset(Vector2 value)
        {
            _offset = value;
            RebuildTween();
        }

        public void SetPulseScale(Vector3 value)
        {
            _pulseScale = value;
            RebuildTween();
        }

        public void SetDuration(float value)
        {
            _duration = value;
            RebuildTween();
        }

        public void SetEase(Ease value)
        {
            _ease = value;
            RebuildTween();
        }
    }
}