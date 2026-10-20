using Abstractions.Components.Zone;
using UnityEngine;

namespace Gameplay.Components.Zone
{
    public class TriggerZone3D : Zone3DBase
    {
        private void OnTriggerEnter(Collider other)
        {
            HandleEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            HandleExit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            HandleStay(other);
        }
    }
}