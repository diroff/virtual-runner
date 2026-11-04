using System;
using Data.PowerUps;
using Gameplay.Models;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps
{
    public sealed class PowerUpProgressionBinder : MonoBehaviour
    {
        [SerializeField] private PowerUpHandler _handler;
        [SerializeField] private PowerUpDefinition[] _definitions = Array.Empty<PowerUpDefinition>();

        private PowerUpProgression _progression;

        public void Initialize(PowerUpProgression progression)
        {
            StopBinding();
            _progression = progression;
            if (_progression == null || _handler == null)
                return;

            foreach (PowerUpDefinition definition in _definitions)
                if (definition != null)
                    _handler.SetLevel(definition, _progression.GetLevel(definition));

            _progression.OnLevelChanged += HandleLevelChanged;
        }

        public void StopBinding()
        {
            if (_progression != null)
                _progression.OnLevelChanged -= HandleLevelChanged;
            _progression = null;
        }

        private void HandleLevelChanged(string id, int level)
        {
            foreach (PowerUpDefinition definition in _definitions)
                if (definition != null && definition.Id == id)
                    _handler.SetLevel(definition, level);
        }

        private void OnDestroy() => StopBinding();
    }
}
