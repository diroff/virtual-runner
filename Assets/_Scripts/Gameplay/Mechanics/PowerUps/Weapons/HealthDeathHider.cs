using Abstractions.Components;
using Abstractions.Components.Interaction;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Weapons
{
    public sealed class HealthDeathHider : MonoBehaviour
    {
        [SerializeField] private HealthComponent _health;
        [SerializeField] private InteractableBase _interactable;

        private void OnEnable()
        {
            if (_health != null)
                _health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            if (_health != null)
                _health.OnDied -= HandleDied;
        }

        private void HandleDied(GameObject victim, GameObject killer)
        {
            if (_interactable != null)
                _interactable.Hide();
        }
    }
}
