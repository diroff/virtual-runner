using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Magnet.Visuals
{
    public abstract class MagnetAreaVisualBase : MonoBehaviour
    {
        public bool IsRunning { get; private set; }

        public bool StartVisual(MagnetTargetingBase targeting, Transform origin)
        {
            StopVisual();
            if (targeting == null || origin == null || !OnStart(targeting))
                return false;

            IsRunning = true;
            UpdateVisual(origin);
            return true;
        }

        public void UpdateVisual(Transform origin)
        {
            if (IsRunning && origin != null)
                OnUpdate(origin);
        }

        public void StopVisual()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            OnStop();
        }

        protected abstract bool OnStart(MagnetTargetingBase targeting);
        protected abstract void OnUpdate(Transform origin);
        protected abstract void OnStop();

        protected virtual void OnDisable() => StopVisual();
    }
}
