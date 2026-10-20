using DG.Tweening;
using UnityEngine;

namespace Gameplay.Components.Animations
{
    public sealed class RotationLoopAnimation : TweenAnimationBase
    {
        [SerializeField] private Vector3 _rotationPerLoop;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private Ease _ease = Ease.Linear;
        [SerializeField] private RotateMode _rotateMode = RotateMode.LocalAxisAdd;
        [SerializeField] private LoopType _loopType = LoopType.Yoyo;
        [SerializeField] private int _loops = -1;

        private Vector3 _initialRotation;

        protected override void CaptureState()
        {
            _initialRotation = _target.localEulerAngles;
        }

        protected override void RestoreState()
        {
            _target.localEulerAngles = _initialRotation;
        }

        protected override Tween BuildTween()
        {
            return _target
                .DOLocalRotate(_rotationPerLoop, _duration, _rotateMode)
                .SetRelative()
                .SetEase(_ease)
                .SetLoops(_loops, _loopType);
        }

        public void SetRotation(Vector3 value)
        {
            _rotationPerLoop = value;
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

        public void SetRotateMode(RotateMode value)
        {
            _rotateMode = value;
            RebuildTween();
        }

        public void SetLoopType(LoopType value)
        {
            _loopType = value;
            RebuildTween();
        }

        public void SetLoops(int value)
        {
            _loops = value;
            RebuildTween();
        }
    }
}