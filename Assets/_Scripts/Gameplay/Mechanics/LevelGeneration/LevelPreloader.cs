using System.Threading;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Mechanics.LevelGeneration
{
    public sealed class LevelPreloader : MonoBehaviour
    {
        [SerializeField, Min(0)] private int _additionalSegmentCount = 1;
        [SerializeField] private bool _preloadOnStart;

        [Inject] private LevelSegmentSpawner _spawner;

        private void Start()
        {
            if (_preloadOnStart)
                Preload();
        }

        public void Preload()
        {
            PreloadAsync(destroyCancellationToken).Forget();
        }

        public async UniTask<bool> PreloadAsync(CancellationToken cancellationToken = default)
        {
            if (_spawner == null)
                return false;

            if (!await _spawner.PrepareLevelAsync(cancellationToken))
                return false;

            return !cancellationToken.IsCancellationRequested && _spawner.EnsureInitialSegments(_additionalSegmentCount);
        }
    }
}
