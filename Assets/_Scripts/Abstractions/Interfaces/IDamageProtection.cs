using UnityEngine;

namespace Abstractions.Interfaces
{
    public interface IDamageProtection
    {
        bool TryPreventDamage(float amount, GameObject sender);
    }
}
