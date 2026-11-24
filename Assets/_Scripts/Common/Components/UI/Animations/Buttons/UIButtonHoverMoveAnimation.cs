using Abstractions.UI;
using DG.Tweening;
using UnityEngine;

namespace Common.Components.UI.Animations.Buttons
{
    [DisallowMultipleComponent]
    public sealed class UIButtonHoverMoveAnimation : UIButtonAnimationBase
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _hoverOffset = new Vector3(0f, 5f, 0f);
        [SerializeField, Min(0f)] private float _duration = 0.15f;
        [SerializeField] private Ease _ease = Ease.OutQuad;

        private RectTransform _rectTransform;
        private Vector3 _initialPosition;
        private bool _isHovered;

        protected override bool InitializeAnimation()
        {
            if (_target == null)
                _target = Button.TargetButton.transform;

            _rectTransform = _target as RectTransform;
            _initialPosition = _rectTransform != null ? _rectTransform.anchoredPosition3D : _target.localPosition;
            _isHovered = false;
            return true;
        }

        protected override void RestoreState()
        {
            SetPosition(_initialPosition);
        }

        protected override void OnButtonStateChanged()
        {
            if (_target == null || _isHovered == Button.IsHovered)
                return;

            _isHovered = Button.IsHovered;
            var position = _initialPosition + (_isHovered ? _hoverOffset : Vector3.zero);
            StopTween();

            if (_duration <= 0f)
            {
                SetPosition(position);
                return;
            }

            Tween tween = _rectTransform != null
                ? _rectTransform.DOAnchorPos3D(position, _duration)
                : _target.DOLocalMove(position, _duration);

            PlayTween(tween.SetEase(_ease));
        }

        private void SetPosition(Vector3 position)
        {
            if (_rectTransform != null)
                _rectTransform.anchoredPosition3D = position;
            else if (_target != null)
                _target.localPosition = position;
        }
    }
}
