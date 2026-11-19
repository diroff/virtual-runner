using Abstractions.Interfaces;
using UnityEngine;

namespace Gameplay.Components.Interaction
{
    public class Interactor : MonoBehaviour, IInteractor
    {
        [SerializeField] private GameObject _interactionTarget;

        public bool IsActive {get; private set;}
        public GameObject Source => _interactionTarget != null ? _interactionTarget : gameObject;

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
