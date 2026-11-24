using Abstractions.UI;
using DG.Tweening;
using UnityEngine;

namespace Common.Components.UI.Animations.Panels
{
    [DisallowMultipleComponent]
    public sealed class UIPanelFadeAnimation : UIPanelAnimationBase
    {
        [SerializeField] private CanvasGroup _target;
        [SerializeField, Range(0f, 1f)] private float _shownAlpha = 1f;
        [SerializeField, Min(0f)] private float _showDuration = 0.25f;
        [SerializeField, Min(0f)] private float _hideDuration = 0.2f;
        [SerializeField] private Ease _showEase = Ease.OutQuad;
        [SerializeField] private Ease _hideEase = Ease.InQuad;

        protected override void OnInitialize(UIPanelBase panel)
        {
            if (_target == null)
                _target = panel.CanvasGroup;
        }

        public override void SetState(bool isVisible)
        {
            if (_target != null)
                _target.alpha = isVisible ? _shownAlpha : 0f;
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

            return _target.DOFade(isVisible ? _shownAlpha : 0f, duration)
                .SetEase(isVisible ? _showEase : _hideEase);
        }
    }
}
