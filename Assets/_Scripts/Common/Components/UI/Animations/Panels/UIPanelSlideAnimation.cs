using Abstractions.UI;
using DG.Tweening;
using UnityEngine;

namespace Common.Components.UI.Animations.Panels
{
    [DisallowMultipleComponent]
    public sealed class UIPanelSlideAnimation : UIPanelAnimationBase
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _hiddenOffset = new Vector3(300f, 0f, 0f);
        [SerializeField, Min(0f)] private float _showDuration = 0.3f;
        [SerializeField, Min(0f)] private float _hideDuration = 0.25f;
        [SerializeField] private Ease _showEase = Ease.OutCubic;
        [SerializeField] private Ease _hideEase = Ease.InCubic;

        private RectTransform _rectTransform;
        private Vector3 _initialPosition;

        protected override void OnInitialize(UIPanelBase panel)
        {
            if (_target == null)
                _target = panel.Root;

            _rectTransform = _target as RectTransform;
            _initialPosition = _rectTransform != null ? _rectTransform.anchoredPosition3D : _target.localPosition;
        }

        public override void SetState(bool isVisible)
        {
            if (_target == null)
                return;

            var position = GetPosition(isVisible);

            if (_rectTransform != null)
                _rectTransform.anchoredPosition3D = position;
            else
                _target.localPosition = position;
        }

        public override Tween BuildTween(bool isVisible)
        {
            if (_target == null)
                return null;

            float duration = isVisible ? _showDuration : _hideDuration;

            if (duration <= 0f)
            {
                SetState(isVisible);
                return null;
            }

            var position = GetPosition(isVisible);
            Tween tween = _rectTransform != null
                ? _rectTransform.DOAnchorPos3D(position, duration)
                : _target.DOLocalMove(position, duration);

            return tween.SetEase(isVisible ? _showEase : _hideEase);
        }

        private Vector3 GetPosition(bool isVisible)
        {
            return isVisible ? _initialPosition : _initialPosition + _hiddenOffset;
        }
    }
}
