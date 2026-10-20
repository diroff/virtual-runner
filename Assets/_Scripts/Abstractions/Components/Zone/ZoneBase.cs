using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Abstractions.Components.Zone
{
    public abstract class ZoneBase<TCollider> : MonoBehaviour where TCollider : Component
    {
        [Header("Settings")]
        [SerializeField] private bool _isActive = true;
        [SerializeField] private bool _oneShot = false;

        [Header("Zone Configuration")]
        [SerializeField] protected LayerMask LayerMask = ~0;
        [SerializeField] protected float StayFrequency = 0f;

        [Header("Inspector Events")]
        [SerializeField] private UnityEvent<GameObject> _onEnterEvent;
        [SerializeField] private UnityEvent<GameObject> _onExitEvent;
        [SerializeField] private UnityEvent<GameObject> _onStayEvent;

        private bool _hasTriggered;

        public event Action<TCollider> OnEnter;
        public event Action<TCollider> OnExit;
        public event Action<TCollider> OnStay;
        public event Action<bool> OnActiveStateChanged;

        protected readonly Dictionary<TCollider, float> StayTimers = new();

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value)
                    return;

                _isActive = value;

                if (!_isActive)
                    StayTimers.Clear();

                OnActiveStateChanged?.Invoke(_isActive);
            }
        }

        protected void HandleEnter(TCollider other)
        {
            if (!IsValid(other))
                return;

            if (_oneShot)
                _hasTriggered = true;

            OnEnter?.Invoke(other);
            _onEnterEvent?.Invoke(other.gameObject);

            if (StayFrequency > 0f)
                StayTimers[other] = 0f;
        }

        protected void HandleExit(TCollider other)
        {
            if (!IsValid(other))
                return;

            OnExit?.Invoke(other);
            _onExitEvent?.Invoke(other.gameObject);

            StayTimers.Remove(other);
        }

        protected void HandleStay(TCollider other)
        {
            if (!IsValid(other))
                return;

            if (StayFrequency <= 0f)
            {
                OnStay?.Invoke(other);
                _onStayEvent?.Invoke(other.gameObject);
                return;
            }

            if (!StayTimers.ContainsKey(other))
                StayTimers[other] = 0f;

            StayTimers[other] += Time.deltaTime;

            if (StayTimers[other] >= 1f / StayFrequency)
            {
                StayTimers[other] = 0f;
                OnStay?.Invoke(other);
                _onStayEvent?.Invoke(other.gameObject);
            }
        }

        protected virtual bool IsValid(TCollider other)
        {
            if (!_isActive)
                return false;

            if (_oneShot && _hasTriggered)
                return false;

            return (LayerMask.value & (1 << other.gameObject.layer)) != 0;
        }
    }
}