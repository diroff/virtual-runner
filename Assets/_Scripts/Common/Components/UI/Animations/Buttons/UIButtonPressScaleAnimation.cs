using DG.Tweening;
using UnityEngine;

namespace Common.Components.UI.Animations.Buttons
{
    [DisallowMultipleComponent]
    public sealed class UIButtonPressScaleAnimation : UIButtonScaleAnimationBase
    {
        [SerializeField, Min(0f)] private float _scaleMultiplier = 0.92f;
        [SerializeField, Min(0f)] private float _duration = 0.08f;
        [SerializeField] private Ease _ease = Ease.OutQuad;

        private bool _isActive;

        protected override bool InitializeAnimation()
        {
            _isActive = false;
            return base.InitializeAnimation();
        }

        protected override void OnButtonStateChanged()
        {
            bool isActive = Button.IsPressed;

            if (_isActive == isActive)
                return;

            _isActive = isActive;
            AnimateScale(isActive ? _scaleMultiplier : 1f, _duration, _ease);
        }
    }
}
