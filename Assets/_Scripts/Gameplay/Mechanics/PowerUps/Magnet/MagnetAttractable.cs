using Abstractions.Components.Interaction;
using Abstractions.Interfaces;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Magnet
{
    public sealed class MagnetAttractable : MonoBehaviour, IRespawnable
    {
        [SerializeField] private InteractableBase _interactable;

        private Vector3 _initialLocalPosition;

        private void Awake()
        {
            _initialLocalPosition = transform.localPosition;
            if (_interactable == null)
                _interactable = GetComponent<InteractableBase>();
        }

        public void Respawn() => transform.localPosition = _initialLocalPosition;

        public bool AttractTo(Vector3 target, float speed, float collectRadius, GameObject interactor, float deltaTime)
        {
            if (!gameObject.activeInHierarchy || _interactable == null || interactor == null)
                return false;

            transform.position = Vector3.MoveTowards(transform.position, target, speed * deltaTime);

            if ((transform.position - target).sqrMagnitude <= collectRadius * collectRadius && _interactable.CanInteract(interactor))
            {
                _interactable.Interact(interactor);
                return false;
            }

            return gameObject.activeInHierarchy;
        }
    }
}
