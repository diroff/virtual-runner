using Gameplay.Components.Interaction;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Mechanics.Creatures
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private List<Interactor> _interactors;

        public void EnableInteractors()
        {
            foreach(var interactor in _interactors)
                interactor.Activate();
        }

        public void DisableInteractors()
        {
            foreach (var interactor in _interactors)
                interactor.Deactivate();
        }
    }
}