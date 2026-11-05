using System;
using System.Collections.Generic;
using Data.PowerUps;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps
{
    public sealed class PowerUpHandler : MonoBehaviour
    {
        [SerializeField] private PowerUpContext _context;
        [SerializeField] private PowerUpBase[] _powerUps;

        private readonly Dictionary<PowerUpDefinition, PowerUpBase> _registered = new();
        private readonly Dictionary<PowerUpSlot, PowerUpBase> _occupied = new();
        private readonly Dictionary<string, int> _levels = new();

        public bool IsHandling { get; private set; }

        public event Action<PowerUpBase> OnStarted;
        public event Action<PowerUpBase> OnRefreshed;
        public event Action<PowerUpBase> OnEnded;

        public void SetLevel(PowerUpDefinition definition, int level)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                return;

            _levels[definition.Id] = Mathf.Clamp(level, 1, Mathf.Max(1, definition.MaxLevel));
        }

        public int GetLevel(PowerUpDefinition definition)
        {
            return definition != null && _levels.TryGetValue(definition.Id, out int level) ? level : 1;
        }

        public bool StartHandling()
        {
            if (IsHandling)
                return true;

            if (_context == null)
                return false;

            _registered.Clear();
            _occupied.Clear();
            HashSet<string> registeredIds = new();

            foreach (PowerUpBase powerUp in _powerUps)
            {
                if (powerUp == null || powerUp.Definition == null || powerUp.Lifetime == null ||
                    string.IsNullOrWhiteSpace(powerUp.Definition.Id) || powerUp.Definition.MaxLevel == 0 ||
                    !registeredIds.Add(powerUp.Definition.Id) ||
                    !_registered.TryAdd(powerUp.Definition, powerUp))
                {
                    Debug.LogError($"{nameof(PowerUpHandler)}: Invalid or duplicate power up.", this);
                    _registered.Clear();
                    return false;
                }
            }

            foreach (PowerUpBase powerUp in _registered.Values)
                powerUp.OnEnded += HandleEnded;

            IsHandling = true;
            return true;
        }

        public void StopHandling()
        {
            if (!IsHandling)
                return;

            IsHandling = false;
            foreach (PowerUpBase powerUp in _registered.Values)
                powerUp.StopPowerUp();

            foreach (PowerUpBase powerUp in _registered.Values)
                powerUp.OnEnded -= HandleEnded;

            _occupied.Clear();
            _registered.Clear();
        }

        public bool CanAcquire(PowerUpDefinition definition)
        {
            if (!IsHandling || definition == null || !_registered.TryGetValue(definition, out PowerUpBase powerUp))
                return false;

            if (powerUp.IsRunning)
                return true;

            return definition.Slot == PowerUpSlot.None || !_occupied.ContainsKey(definition.Slot);
        }

        public bool TryAcquire(PowerUpDefinition definition)
        {
            if (!CanAcquire(definition))
                return false;

            PowerUpBase powerUp = _registered[definition];
            if (powerUp.IsRunning)
            {
                powerUp.RefreshPowerUp();
                OnRefreshed?.Invoke(powerUp);
                return true;
            }

            if (!powerUp.StartPowerUp(_context, GetLevel(definition)))
                return false;

            if (definition.Slot != PowerUpSlot.None)
                _occupied.Add(definition.Slot, powerUp);

            OnStarted?.Invoke(powerUp);
            return true;
        }

        private void HandleEnded(PowerUpBase powerUp)
        {
            if (powerUp.Definition.Slot != PowerUpSlot.None)
                _occupied.Remove(powerUp.Definition.Slot);

            OnEnded?.Invoke(powerUp);
        }

        private void OnDisable() => StopHandling();
    }
}
