using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data.PowerUps;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Lifetimes
{
    public sealed class TimedPowerUpLifetime : PowerUpLifetimeBase
    {
        private CancellationTokenSource _timer;
        private float _remaining;
        private float _startedAt;

        public bool IsPaused { get; private set; }
        public float Remaining => !IsRunning ? 0f : IsPaused
            ? _remaining
            : Mathf.Max(0f, _remaining - (Time.time - _startedAt));

        protected override bool CanStart(PowerUpLevelSettings settings) => settings.Duration > 0f;

        protected override void OnStart(PowerUpLevelSettings settings)
        {
            _remaining = settings.Duration;
            IsPaused = false;
            StartCountdown();
        }

        protected override void OnRefresh(PowerUpLevelSettings settings)
        {
            _remaining = settings.Duration;
            if (!IsPaused)
                StartCountdown();
        }

        protected override void OnStop()
        {
            CancelCountdown();
            _remaining = 0f;
            _startedAt = 0f;
            IsPaused = false;
        }

        public void Pause()
        {
            if (!IsRunning || IsPaused)
                return;

            _remaining = Remaining;
            IsPaused = true;
            CancelCountdown();

            if (_remaining <= 0f)
                Expire();
            else
                NotifyChanged();
        }

        public void Resume()
        {
            if (!IsRunning || !IsPaused)
                return;

            IsPaused = false;
            StartCountdown();
            NotifyChanged();
        }

        private void StartCountdown()
        {
            CancelCountdown();
            _startedAt = Time.time;
            _timer = new CancellationTokenSource();
            WaitForExpiration(_remaining, _timer.Token).Forget();
        }

        private void CancelCountdown()
        {
            _timer?.Cancel();
            _timer?.Dispose();
            _timer = null;
        }

        private async UniTask WaitForExpiration(float duration, CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: cancellationToken);
                if (!cancellationToken.IsCancellationRequested && IsRunning && !IsPaused)
                    Expire();
            }
            catch (OperationCanceledException) { }
        }
    }
}
