using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Abstractions.Mechanics.Lifecycle
{
    public abstract class LevelFinisher : MonoBehaviour
    {
        [Header("Base Events")]
        [SerializeField] private UnityEvent _onBeforeFinish;
        [SerializeField] private UnityEvent _onAfterFinish;

        private bool _isFinished;
        private CancellationTokenSource _cts;

        protected virtual void Awake()
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        }

        protected virtual void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public async UniTaskVoid ExecuteFinishAsync(CancellationToken cancellationToken = default)
        {
            if (_isFinished) return;
            _isFinished = true;

            _onBeforeFinish?.Invoke();
            OnBeforeFinish();

            await OnFinishProcessAsync(_cts.Token);

            OnAfterFinish();
            _onAfterFinish?.Invoke();
        }

        protected virtual void OnBeforeFinish() { }
        protected abstract UniTask OnFinishProcessAsync(CancellationToken cancellationToken);
        protected virtual void OnAfterFinish() { }
    }
}