using DG.Tweening;
using UnityEngine;

namespace Gameplay.Components.Animations
{
    public sealed class ScaleLoopAnimation : TweenAnimationBase
    {
        [SerializeField] private Vector3 _targetScale = Vector3.one * 1.5f;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private Ease _ease = Ease.Linear;

        private Vector3 _initialScale;

        protected override void CaptureState()
        {
            _initialScale = _target.localScale;
        }

        protected override void RestoreState()
        {
            _target.localScale = _initialScale;
        }

        protected override Tween BuildTween()
        {
            return _target
                .DOScale(_targetScale, _duration)
                .SetEase(_ease)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void SetTargetScale(Vector3 value)
        {
            _targetScale = value;
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