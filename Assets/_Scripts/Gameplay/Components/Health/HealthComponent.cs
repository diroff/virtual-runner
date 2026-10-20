using Abstractions.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Abstractions.Components
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _baseHealth;

        public float CurrentHealth { get; protected set; }
        public float MaxHealth { get; protected set; }

        public bool IsDead { get; protected set; }

        public UnityAction<float, float> OnHealthChanged;
        public UnityAction<GameObject> OnDamage;
        public UnityAction<GameObject> OnHeal;
        public UnityAction<GameObject, GameObject> OnDied;
        public UnityAction<GameObject> OnRevived;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            MaxHealth = _baseHealth;
            CurrentHealth = MaxHealth;

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            IsDead = false;
        }

        public void Damage(float value, GameObject sender)
        {
            if (IsDead)
                return;

            if (value <= 0)
                return;

            CurrentHealth = Mathf.Clamp(CurrentHealth - value, 0, MaxHealth);

            if (CurrentHealth <= 0)
                Die(sender);

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            OnDamage?.Invoke(sender);
        }

        public void Heal(float value, GameObject sender)
        {
            if (IsDead)
                return;

            CurrentHealth = Mathf.Clamp(CurrentHealth + value, 0, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            OnHeal?.Invoke(sender);
        }

        public void ChangeMaxValue(float newValue, GameObject sender, bool addDifference = false)
        {
            if (newValue <= 0)
                return;

            float oldMax = MaxHealth;
            MaxHealth = newValue;

            if (addDifference)
            {
                float delta = newValue - oldMax;

                if(delta > 0)
                    CurrentHealth += delta;
            }

            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void Revive(GameObject sender)
        {
            if (!IsDead)
                return;

            IsDead = false;
            Heal(MaxHealth, sender);

            OnRevived?.Invoke(sender);
        }

        private void Die(GameObject killer)
        {
            if (IsDead)
                return;

            IsDead = true;

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            OnDied?.Invoke(gameObject, killer);
        }
    }
}