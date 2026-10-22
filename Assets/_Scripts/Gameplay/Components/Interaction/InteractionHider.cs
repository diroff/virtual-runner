using Abstractions.Components.Interaction;
using UnityEngine;

namespace Gameplay.Components.Interaction
{
    public class InteractionHider : MonoBehaviour
    {
        [SerializeField] private InteractableBase _interactable;

        private void Awake()
        {
            if (_interactable == null)
                _interactable = GetComponentInParent<InteractableBase>();
        }

        private void OnEnable()
        {
            if (_interactable != null)
                _interactable.OnInteracted += HandleInteracted;
        }

        private void OnDisable()
        {
            if (_interactable != null)
                _interactable.OnInteracted -= HandleInteracted;
        }

        private void HandleInteracted(GameObject interactor)
        {
            _interactable.Hide();
        }
    }
}