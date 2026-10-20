using Abstractions.Components.Zone;
using UnityEngine;

namespace Gameplay.Components.Zone
{
    public class CollisionZone3D : Zone3DBase
    {
        private void OnCollisionEnter(Collision collision)
        {
            HandleEnter(collision.collider);
        }

        private void OnCollisionExit(Collision collision)
        {
            HandleExit(collision.collider);
        }

        private void OnCollisionStay(Collision collision)
        {
            HandleStay(collision.collider);
        }
    }
}