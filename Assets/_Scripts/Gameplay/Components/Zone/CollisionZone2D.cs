using Abstractions.Components.Zone;
using UnityEngine;

namespace Gameplay.Components.Zone
{
    public class CollisionZone2D : Zone2DBase
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleEnter(collision.collider);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            HandleExit(collision.collider);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            HandleStay(collision.collider);
        }
    }
}