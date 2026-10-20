using Abstractions.Components.Zone;
using UnityEngine;

namespace Gameplay.Components.Zone
{
    public class TriggerZone2D : Zone2DBase
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleEnter(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            HandleExit(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            HandleStay(other);
        }
    }
}