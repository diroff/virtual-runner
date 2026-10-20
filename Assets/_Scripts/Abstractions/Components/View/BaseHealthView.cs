using UnityEngine;

namespace Abstractions.Components.View
{
    public abstract class BaseHealthView : MonoBehaviour
    {
        [SerializeField] protected HealthComponent Health;

        protected virtual void OnEnable()
        {
            Health.OnHealthChanged += UpdateHealthView;
        }

        protected virtual void OnDisable()
        {
            Health.OnHealthChanged -= UpdateHealthView;
        }

        protected virtual void Start()
        {
            InitializeHealthView();
        }

        protected virtual void InitializeHealthView()
        {
            UpdateHealthView(Health.CurrentHealth, Health.MaxHealth);
        }

        protected abstract void UpdateHealthView(float currentValue, float maxValue);
    }
}