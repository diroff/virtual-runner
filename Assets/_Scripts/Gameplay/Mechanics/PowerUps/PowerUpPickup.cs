using Abstractions.Components.Collectables;
using Data.PowerUps;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps
{
    public sealed class PowerUpPickup : CollectableBase
    {
        [SerializeField] private PowerUpDefinition _definition;

        public override bool CanInteract(GameObject interactor)
        {
            if (!base.CanInteract(interactor))
                return false;

            PowerUpHandler handler = interactor.GetComponentInParent<PowerUpHandler>();
            Debug.Log(handler.name);
            return handler != null && handler.CanAcquire(_definition);
        }

        protected override void HandleInteraction(GameObject interactor)
        {
            PowerUpHandler handler = interactor.GetComponentInParent<PowerUpHandler>();

            Debug.Log(handler.name);

            if (handler == null || !handler.TryAcquire(_definition))
                return;

            base.HandleInteraction(interactor);
            Hide();
        }
    }
}
