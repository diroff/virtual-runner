using DG.Tweening;
using UnityEngine;

namespace Gameplay.Components.Animations
{
    public sealed class ShakeLoopAnimation : TweenAnimationBase
    {
        [SerializeField] private Vector3 _offset = new(0.5f, 0.5f, 0f);
        [SerializeField] private Vector3 _pulseScale = Vector3.one * 1.1f;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private Ease _ease = Ease.InOutSine;

        private Vector3 _initialPosition;
        private Vector3 _initialScale;

        protected override void CaptureState()
        {
            _initialPosition = _target.localPosition;
            _initialScale = _target.localScale;
        }

        protected override void RestoreState()
        {
            _target.localPosition = _initialPosition;
            _target.localScale = _initialScale;
        }

        protected override Tween BuildTween()
        {
            return DOTween.Sequence()
                .Append(_target.DOLocalMove(_initialPosition + _offset, _duration).SetEase(_ease))
                .Join(_target.DOScale(_pulseScale, _duration).SetEase(_ease))
                .Append(_target.DOLocalMove(_initialPosition - _offset, _duration).SetEase(_ease))
                .Join(_target.DOScale(_initialScale, _duration).SetEase(_ease))
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void SetOffset(Vector3 value)
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