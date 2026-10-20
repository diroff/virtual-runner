using UnityEngine;

namespace Abstractions.Interfaces
{
    public interface IInteractable
    {
        public void Interact(GameObject interactor);
        public bool CanInteract(GameObject interactor) => true;
    }
}