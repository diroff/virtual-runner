using Abstractions.Interfaces;
using UnityEngine;

namespace Gameplay.Components.Interaction
{
    public class Interactor : MonoBehaviour, IInteractor
    {
        [SerializeField] private GameObject _interactionTarget;

        public GameObject Source => _interactionTarget != null ? _interactionTarget : gameObject;
    }
}
