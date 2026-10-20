using Abstractions.Components.View;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Components.View
{
    [RequireComponent(typeof(CanvasGroup))]
    public class HealthSliderView : BaseHealthView
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _animationSpeed = 100f;
        [SerializeField] private float _fadeDuration = 0.2f;

        private Sequence _animationSequence;

        protected virtual void Awake()
        {
            if (_slider == null)
                _slider = GetComponentInChildren<Slider>();

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            _animationSequence?.Kill();
        }

        protected override void InitializeHealthView()
        {
            _slider.maxValue = Health.MaxHealth;
            _slider.value = Health.CurrentHealth;

            bool isFull = Mathf.Approximately(Health.CurrentHealth, Health.MaxHealth);
            _canvasGroup.alpha = isFull ? 0f : 1f;
        }

        protected override void UpdateHealthView(float currentValue, float maxValue)
        {
            _animationSequence?.Kill();
            _animationSequence = DOTween.Sequence().SetLink(gameObject);

            bool wasFull = Mathf.Approximately(_slider.value, _slider.maxValue);
            bool willBeFull = Mathf.Approximately(currentValue, maxValue);

            _slider.maxValue = maxValue;

            if (!wasFull && willBeFull)
            {
                AppendFillAnimation(currentValue);
                _animationSequence.Append(_canvasGroup.DOFade(0f, _fadeDuration));
            }
            else if (wasFull && !willBeFull)
            {
                _slider.value = maxValue;
                _canvasGroup.alpha = 0f;
                _animationSequence.Append(_canvasGroup.DOFade(1f, _fadeDuration));
                AppendFillAnimation(currentValue);
            }
            else
            {
                HandlePartialTransition(currentValue, willBeFull);
            }
        }

        private void AppendFillAnimation(float targetValue)
        {
            float duration = Mathf.Abs(_slider.value - targetValue) / _animationSpeed;

            if (duration > 0f)
                _animationSequence.Append(_slider.DOValue(targetValue, duration).SetEase(Ease.Linear));
            else
                _slider.value = targetValue;
        }

        private void HandlePartialTransition(float targetValue, bool willBeFull)
        {
            if (willBeFull)
            {
                _slider.value = targetValue;
                _canvasGroup.alpha = 0f;
            }
            else
            {
                _canvasGroup.alpha = 1f;
                AppendFillAnimation(targetValue);
            }
        }
    }
}