using Abstractions.UI;
using DG.Tweening;
using UnityEngine;

namespace Common.Components.UI.Animations.Panels
{
    [DisallowMultipleComponent]
    public sealed class UIPanelScaleAnimation : UIPanelAnimationBase
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _hiddenScale = Vector3.one * 0.85f;
        [SerializeField, Min(0f)] private float _showDuration = 0.25f;
        [SerializeField, Min(0f)] private float _hideDuration = 0.2f;
        [SerializeField] private Ease _showEase = Ease.OutCubic;
        [SerializeField] private Ease _hideEase = Ease.InCubic;

        private Vector3 _initialScale;

        protected override void OnInitialize(UIPanelBase panel)
        {
            if (_target == null)
                _target = panel.Root;

            _initialScale = _target.localScale;
        }

        public override void SetState(bool isVisible)
        {
            if (_target != null)
                _target.localScale = GetScale(isVisible);
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

            return _target.DOScale(GetScale(isVisible), duration)
                .SetEase(isVisible ? _showEase : _hideEase);
        }

        private Vector3 GetScale(bool isVisible)
        {
            return isVisible ? _initialScale : Vector3.Scale(_initialScale, _hiddenScale);
        }
    }
}
