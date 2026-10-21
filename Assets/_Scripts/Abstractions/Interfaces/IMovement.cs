using UnityEngine;

namespace Abstractions.Interfaces
{
    public interface IMovement
    {
        public bool IsMoving { get; }

        public bool StartMovement();
        public void StopMovement();
        public void SetDirection(Vector3 direction);
        public void SetDirectionReference(Transform reference);
    }
}