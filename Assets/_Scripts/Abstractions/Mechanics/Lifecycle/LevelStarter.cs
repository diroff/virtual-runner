using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Abstractions.Mechanics.Lifecycle
{
    public abstract class LevelStarter : MonoBehaviour
    {
        [Header("Base Events")]
        [SerializeField] private UnityEvent _onBeforeStart;
        [SerializeField] private UnityEvent _onAfterStart;

        [Header("Settings")]
        [SerializeField] private bool _autoStartOnAwake = true;

        private bool _isStarted;

        protected virtual void Start()
        {
            if (_autoStartOnAwake)
                ExecuteStartAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        public async UniTaskVoid ExecuteStartAsync(CancellationToken cancellationToken = default)
        {
            if (_isStarted) return;
            _isStarted = true;

            _onBeforeStart?.Invoke();
            OnBeforeStart();

            await OnStartProcessAsync(cancellationToken);

            OnAfterStart();
            _onAfterStart?.Invoke();
        }

        protected virtual void OnBeforeStart() { }
        protected abstract UniTask OnStartProcessAsync(CancellationToken cancellationToken);
        protected virtual void OnAfterStart() { }
    }
}