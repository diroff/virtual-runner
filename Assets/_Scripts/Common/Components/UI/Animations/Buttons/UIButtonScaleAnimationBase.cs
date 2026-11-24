using Abstractions.UI;
using DG.Tweening;
using UnityEngine;

namespace Common.Components.UI.Animations.Buttons
{
    public abstract class UIButtonScaleAnimationBase : UIButtonAnimationBase
    {
        [SerializeField] private Transform _target;
        private UIButtonScaleMixer _mixer;
        private float _multiplier = 1f;

        protected override bool InitializeAnimation()
        {
            if (_target == null)
                _target = Button.TargetButton.transform;

            _mixer = _target.GetComponent<UIButtonScaleMixer>();

            if (_mixer == null)
                _mixer = _target.gameObject.AddComponent<UIButtonScaleMixer>();

            _multiplier = 1f;
            _mixer.Register(this);
            return true;
        }

        protected override void RestoreState()
        {
            if (_mixer != null)
                _mixer.Unregister(this);

            _multiplier = 1f;
            _mixer = null;
        }

        protected void SetMultiplier(float value)
        {
            _multiplier = value;

            if (_mixer != null)
                _mixer.SetMultiplier(this, value);
        }

        protected Tween BuildScaleTween(float value, float duration, Ease ease)
        {
            return DOTween.To(() => _multiplier, SetMultiplier, value, duration).SetEase(ease);
        }

        protected void AnimateScale(float value, float duration, Ease ease)
        {
            StopTween();

            if (duration <= 0f || Mathf.Approximately(_multiplier, value))
            {
                SetMultiplier(value);
                return;
            }

            PlayTween(BuildScaleTween(value, duration, ease));
        }
    }
}
