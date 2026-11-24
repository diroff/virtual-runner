using DG.Tweening;
using UnityEngine;

namespace Abstractions.UI
{
    public abstract class UIPanelAnimationBase : MonoBehaviour
    {
        private bool _isInitialized;

        public void Initialize(UIPanelBase panel)
        {
            if (_isInitialized)
                return;

            OnInitialize(panel);
            _isInitialized = true;
        }

        protected abstract void OnInitialize(UIPanelBase panel);
        public abstract void SetState(bool isVisible);
        public abstract Tween BuildTween(bool isVisible);
    }
}
