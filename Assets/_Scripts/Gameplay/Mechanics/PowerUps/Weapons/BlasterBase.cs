using System;
using Gameplay.Mechanics.PowerUps.Lifetimes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Mechanics.PowerUps.Weapons
{
    public abstract class BlasterBase : MonoBehaviour
    {
        [SerializeField] private InputActionReference _fireAction;
        [SerializeField, Min(0f)] private float _secondsBetweenShots = 0.25f;

        private ChargePowerUpLifetime _ammunition;
        private PowerUpContext _context;
        private bool _actionEnabledHere;
        private float _nextShotAt;

        public bool IsShootingEnabled { get; private set; }
        public event Action OnShot;

        public bool StartShooting(PowerUpContext context, ChargePowerUpLifetime ammunition)
        {
            if (IsShootingEnabled || context == null || ammunition == null || _fireAction == null || _fireAction.action == null)
                return false;

            if (!CanStart())
                return false;

            _context = context;
            _ammunition = ammunition;
            _nextShotAt = 0f;

            InputAction action = _fireAction.action;
            _actionEnabledHere = !action.enabled;
            if (_actionEnabledHere)
                action.Enable();

            action.performed += HandleFire;
            IsShootingEnabled = true;
            return true;
        }

        public void StopShooting()
        {
            if (!IsShootingEnabled)
                return;

            IsShootingEnabled = false;
            InputAction action = _fireAction.action;
            action.performed -= HandleFire;

            if (_actionEnabledHere)
                action.Disable();

            _actionEnabledHere = false;
            _ammunition = null;
            _context = null;
        }

        protected abstract bool CanStart();
        protected abstract bool Shoot(PowerUpContext context);

        private void HandleFire(InputAction.CallbackContext input)
        {
            if (!IsShootingEnabled || _ammunition == null || !_ammunition.IsRunning ||
                _ammunition.Charges == 0 || Time.time < _nextShotAt)
                return;

            if (!Shoot(_context))
                return;

            _nextShotAt = Time.time + _secondsBetweenShots;
            _ammunition.TryConsume();
            OnShot?.Invoke();
        }

        protected virtual void OnDisable() => StopShooting();
    }
}
