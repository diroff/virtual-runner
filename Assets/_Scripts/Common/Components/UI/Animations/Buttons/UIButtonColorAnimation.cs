using Abstractions.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Components.UI.Animations.Buttons
{
    [DisallowMultipleComponent]
    public sealed class UIButtonColorAnimation : UIButtonAnimationBase
    {
        [SerializeField] private Graphic _target;
        [SerializeField] private Color _hoverColor = new Color(0.9f, 0.9f, 1f);
        [SerializeField] private Color _pressedColor = new Color(0.7f, 0.7f, 0.85f);
        [SerializeField] private Color _selectedColor = new Color(0.85f, 0.85f, 1f);
        [SerializeField] private Color _disabledColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField, Min(0f)] private float _duration = 0.15f;
        [SerializeField] private Ease _ease = Ease.OutQuad;

        private Color _initialColor;
        private Color _targetColor;

        protected override bool InitializeAnimation()
        {
            if (_target == null)
                _target = Button.TargetButton.targetGraphic;

            if (_target == null)
                return false;

            _initialColor = _target.color;
            _targetColor = _initialColor;
            return true;
        }

        protected override void RestoreState()
        {
            if (_target != null)
                _target.color = _initialColor;
        }

        protected override void OnButtonStateChanged()
        {
            if (_target == null)
                return;

            var color = GetColor();

            if (_targetColor == color)
                return;

            _targetColor = color;
            StopTween();

            if (_duration <= 0f)
            {
                _target.color = color;
                return;
            }

            PlayTween(_target.DOColor(color, _duration).SetEase(_ease));
        }

        private Color GetColor()
        {
            Color multiplier = Color.white;

            if (!Button.IsInteractable)
                multiplier = _disabledColor;
            else if (Button.IsPressed)
                multiplier = _pressedColor;
            else if (Button.IsHovered)
                multiplier = _hoverColor;
            else if (Button.IsSelected)
                multiplier = _selectedColor;

            return _initialColor * multiplier;
        }
    }
}
