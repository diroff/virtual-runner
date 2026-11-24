using Abstractions.UI;
using DG.Tweening;
using UnityEngine;

namespace Common.Components.UI.Animations.Buttons
{
    [DisallowMultipleComponent]
    public sealed class UIButtonFadeAnimation : UIButtonAnimationBase
    {
        [SerializeField] private CanvasGroup _target;
        [SerializeField, Range(0f, 1f)] private float _hoverAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float _pressedAlpha = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _disabledAlpha = 0.5f;
        [SerializeField, Min(0f)] private float _duration = 0.15f;
        [SerializeField] private Ease _ease = Ease.OutQuad;

        private float _initialAlpha;
        private float _targetAlpha;

        protected override bool InitializeAnimation()
        {
            if (_target == null)
                _target = Button.TargetButton.GetComponent<CanvasGroup>();

            if (_target == null)
                _target = Button.TargetButton.gameObject.AddComponent<CanvasGroup>();

            _initialAlpha = _target.alpha;
            _targetAlpha = _initialAlpha;
            return true;
        }

        protected override void RestoreState()
        {
            if (_target != null)
                _target.alpha = _initialAlpha;
        }

        protected override void OnButtonStateChanged()
        {
            if (_target == null)
                return;

            float alpha = GetAlpha();

            if (Mathf.Approximately(_targetAlpha, alpha))
                return;

            _targetAlpha = alpha;
            StopTween();

            if (_duration <= 0f)
            {
                _target.alpha = alpha;
                return;
            }

            PlayTween(_target.DOFade(alpha, _duration).SetEase(_ease));
        }

        private float GetAlpha()
        {
            if (!Button.IsInteractable)
                return _initialAlpha * _disabledAlpha;
            if (Button.IsPressed)
                return _initialAlpha * _pressedAlpha;
            if (Button.IsHovered)
                return _initialAlpha * _hoverAlpha;

            return _initialAlpha;
        }
    }
}
