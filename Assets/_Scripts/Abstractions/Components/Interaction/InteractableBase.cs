using Abstractions.Components.Zone;
using Abstractions.Interfaces;
using Gameplay.Components.Interaction;
using System;
using UnityEngine;

namespace Abstractions.Components.Interaction
{
    public abstract class InteractableBase : MonoBehaviour, IInteractable, IRespawnable
    {
        [Header("Interaction")]
        [SerializeField] private Zone3DBase _interactionZone;

        public event Action<GameObject> OnInteracted;
        public event Action OnHidden;
        public event Action OnRespawned;

        protected virtual void Awake()
        {
            if (_interactionZone == null)
                _interactionZone = GetComponent<Zone3DBase>();
        }

        protected virtual void OnEnable()
        {
            if (_interactionZone != null)
                _interactionZone.OnEnter += HandleEnter;
        }

        protected virtual void OnDisable()
        {
            if (_interactionZone != null)
                _interactionZone.OnEnter -= HandleEnter;
        }

        public virtual bool CanInteract(GameObject interactor)
        {
            return interactor != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
                return;

            HandleInteraction(interactor);
            OnInteracted?.Invoke(interactor);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            OnHidden?.Invoke();
            gameObject.SetActive(false);
        }

        public void Respawn()
        {
            ResetState();
            gameObject.SetActive(true);
            OnRespawned?.Invoke();
        }

        protected abstract void HandleInteraction(GameObject interactor);

        protected virtual void ResetState()
        {
            if (_interactionZone != null)
                _interactionZone.ResetRuntimeState();
        }

        private void HandleEnter(Collider other)
        {
            var interactor = other.GetComponentInParent<Interactor>();

            if (interactor != null)
                Interact(interactor.Source);
        }
    }
}