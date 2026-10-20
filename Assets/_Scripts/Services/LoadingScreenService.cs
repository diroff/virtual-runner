using System.Threading;
using Abstractions.Interfaces;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Reflex.Attributes;
using UnityEngine;

namespace Services
{
    public class LoadingScreenService : IService
    {
        private readonly Material _tvNoiseMaterial;
        private readonly Vector2 _defaultNoiseRange;
        private readonly float _defaultNoiseSpeed;

        [Inject] private AudioEffectsService _audioEffectsService;

        private CancellationTokenSource _ambientNoiseCancellationTokenSource;
        private static readonly int IntensityProperty = Shader.PropertyToID("_Intensity");

        public LoadingScreenService(Material tvNoiseMaterial, Vector2 defaultNoiseRange, float defaultNoiseSpeed = 2f)
        {
            _tvNoiseMaterial = tvNoiseMaterial;
            _defaultNoiseRange = defaultNoiseRange;
            _defaultNoiseSpeed = defaultNoiseSpeed;

            if (_tvNoiseMaterial != null)
            {
                _tvNoiseMaterial.SetFloat(IntensityProperty, 0f);
            }
        }

        public UniTask Initialize()
        {
            StartAmbientNoiseFluctuation(_defaultNoiseRange, _defaultNoiseSpeed);
            return UniTask.CompletedTask;
        }

        public async UniTask FadeInAsync(float duration = 0.5f)
        {
            _audioEffectsService?.PlayTvNoise();

            StopAmbientNoiseFluctuation();

            if (_tvNoiseMaterial != null)
            {
                await _tvNoiseMaterial.DOFloat(1f, IntensityProperty, duration)
                    .SetUpdate(true)
                    .ToUniTask();
            }
        }

        public async UniTask FadeOutAsync(float duration = 0.5f)
        {
            if (_tvNoiseMaterial != null)
            {
                await _tvNoiseMaterial.DOFloat(0f, IntensityProperty, duration)
                    .SetUpdate(true)
                    .ToUniTask();

                StartAmbientNoiseFluctuation(_defaultNoiseRange, _defaultNoiseSpeed);
            }
        }

        public void StartAmbientNoiseFluctuation(Vector2 intensityRange, float speed)
        {
            StopAmbientNoiseFluctuation();
            _ambientNoiseCancellationTokenSource = new CancellationTokenSource();
            FluctuationLoopAsync(intensityRange, speed, _ambientNoiseCancellationTokenSource.Token).Forget();
        }

        public void StopAmbientNoiseFluctuation()
        {
            if (_ambientNoiseCancellationTokenSource != null)
            {
                _ambientNoiseCancellationTokenSource.Cancel();
                _ambientNoiseCancellationTokenSource.Dispose();
                _ambientNoiseCancellationTokenSource = null;
            }
        }

        private async UniTaskVoid FluctuationLoopAsync(Vector2 range, float speed, CancellationToken token)
        {
            if (_tvNoiseMaterial == null) return;
            float seed = UnityEngine.Random.Range(0f, 100f);

            while (!token.IsCancellationRequested)
            {
                float t = Time.unscaledTime * speed + seed;
                float perlinValue = Mathf.PerlinNoise(t, 0f);
                float currentIntensity = Mathf.Lerp(range.x, range.y, perlinValue);

                _tvNoiseMaterial.SetFloat(IntensityProperty, currentIntensity);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
    }
}