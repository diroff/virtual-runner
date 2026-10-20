using Abstractions.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Abstractions.Components.Damage
{
    public class DamageComponent : MonoBehaviour
    {
        [SerializeField] private float _baseDamage;
        [SerializeField] protected bool _canDamageByStart = true;

        private bool _isDamageEnabled;

        public UnityAction<IDamageable, float> OnDealDamage;

        protected virtual void Start()
        {
            if (_canDamageByStart)
                EnableDamage();
        }

        public void EnableDamage()
        {
            _isDamageEnabled = true;
        }

        public void DisableDamage()
        {
            _isDamageEnabled = false;
        }

        public void DealDamage(IDamageable damageable, float value)
        {
            var damageValue = value;

            damageable.Damage(damageValue, gameObject);
            OnDealDamage?.Invoke(damageable, damageValue);
        }

        public bool CanDamage(GameObject obj)
        {
            return _isDamageEnabled;
        }
    }
}