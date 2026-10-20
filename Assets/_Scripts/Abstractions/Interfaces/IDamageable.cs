using UnityEngine;

namespace Abstractions.Interfaces
{
    public interface IDamageable
    {
        public void Damage(float value, GameObject sender);
    }
}