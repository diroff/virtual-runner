using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Magnet
{
    public abstract class MagnetTargetingBase : MonoBehaviour
    {
        public abstract void FindTargets(Transform source, List<MagnetAttractable> results);
        public abstract bool IsAimedAt(Transform source, Vector3 originalTargetPosition);
    }
}
