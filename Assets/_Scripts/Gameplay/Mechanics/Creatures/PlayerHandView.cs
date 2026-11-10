using Data.PowerUps;
using Gameplay.Mechanics.PowerUps;
using UnityEngine;

namespace Gameplay.Mechanics.Creatures
{
    public class PlayerHandView : MonoBehaviour
    {
        [SerializeField] private PowerUpHandler _powerUpHandler;
        [SerializeField] private PowerUpSlot _slot;
        [SerializeField] private Animator _animator;

        private void OnEnable()
        {
            _powerUpHandler.OnStarted += OnPowerUpStarted;
            _powerUpHandler.OnEnded += OnPowerUpEnded;
        }

        private void OnDisable()
        {
            _powerUpHandler.OnStarted -= OnPowerUpStarted;
            _powerUpHandler.OnEnded -= OnPowerUpEnded;
        }

        private void OnPowerUpStarted(PowerUpBase powerUp)
        {
            if (powerUp.Definition.Slot != _slot)
                return;

            _animator.SetBool("Equip", true);
        }

        private void OnPowerUpEnded(PowerUpBase powerUp)
        {
            if (powerUp.Definition.Slot != _slot)
                return;

            _animator.SetBool("Equip", false);
        }
    }
}