using Abstractions.Components.Interaction;
using UnityEngine;

namespace Abstractions.Components.Collectables
{
    public abstract class CollectableBase : InteractableBase
    {
        private bool _isCollected;

        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor) && !_isCollected;
        }

        protected override void HandleInteraction(GameObject interactor)
        {
            _isCollected = true;
        }

        protected override void ResetState()
        {
            base.ResetState();
            _isCollected = false;
        }
    }
}