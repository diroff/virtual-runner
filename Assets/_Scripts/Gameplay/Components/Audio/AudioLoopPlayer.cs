using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Gameplay.Components.Audio
{
    public sealed class AudioLoopPlayer : AudioPlayer
    {
        [Header("Loop Settings")]
        [SerializeField] private bool _playOnStart = true;
        [SerializeField] private Vector2 _delayRange = new(0.5f, 1.5f);

        private CancellationTokenSource _loopCancellationSource;

        private void Start()
        {
            if (_playOnStart)
                StartLoop();
        }

        public void SetPlayingState(bool active)
        {
            if (active)
                StartLoop();
            else
                StopLoop();
        }

        public void StartLoop()
        {
            if (_loopCancellationSource != null || !HasClips)
                return;

            _loopCancellationSource = new CancellationTokenSource();
            PlayLoopAsync(_loopCancellationSource).Forget();
        }

        public void StopLoop()
        {
            if (_loopCancellationSource != null)
            {
                var cancellationSource = _loopCancellationSource;
                _loopCancellationSource = null;
                cancellationSource.Cancel();
            }

            Stop();
        }

        private async UniTask PlayLoopAsync(CancellationTokenSource cancellationSource)
        {
            CancellationToken cancellationToken = cancellationSource.Token;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await PlayAsync(cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    float minDelay = Mathf.Min(_delayRange.x, _delayRange.y);
                    float maxDelay = Mathf.Max(_delayRange.x, _delayRange.y);
                    float delay = UnityEngine.Random.Range(minDelay, maxDelay);

                    if (delay > 0f)
                        await UniTask.Delay(TimeSpan.FromSeconds(delay),cancellationToken: cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (_loopCancellationSource == cancellationSource)
                    _loopCancellationSource = null;

                cancellationSource.Dispose();
            }
        }

        private void OnDestroy()
        {
            StopLoop();
        }
    }
}