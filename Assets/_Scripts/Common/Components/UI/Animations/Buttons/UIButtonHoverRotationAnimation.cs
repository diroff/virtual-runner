using Abstractions.UI;
using DG.Tweening;
using UnityEngine;

namespace Common.Components.UI.Animations.Buttons
{
    [DisallowMultipleComponent]
    public sealed class UIButtonHoverRotationAnimation : UIButtonAnimationBase
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _hoverRotation = new Vector3(0f, 0f, 5f);
        [SerializeField, Min(0f)] private float _duration = 0.15f;
        [SerializeField] private Ease _ease = Ease.OutQuad;

        private Quaternion _initialRotation;
        private bool _isHovered;

        protected override bool InitializeAnimation()
        {
            if (_target == null)
                _target = Button.TargetButton.transform;

            _initialRotation = _target.localRotation;
            _isHovered = false;
            return true;
        }

        protected override void RestoreState()
        {
            if (_target != null)
                _target.localRotation = _initialRotation;
        }

        protected override void OnButtonStateChanged()
        {
            if (_target == null || _isHovered == Button.IsHovered)
                return;

            _isHovered = Button.IsHovered;
            var rotation = _initialRotation * Quaternion.Euler(_isHovered ? _hoverRotation : Vector3.zero);
            StopTween();

            if (_duration <= 0f)
            {
                _target.localRotation = rotation;
                return;
            }

            PlayTween(_target.DOLocalRotateQuaternion(rotation, _duration).SetEase(_ease));
        }
    }
}
