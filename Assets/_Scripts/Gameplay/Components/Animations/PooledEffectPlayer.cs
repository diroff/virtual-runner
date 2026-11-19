using Cysharp.Threading.Tasks;
using Services;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Components.Animations
{
    public enum EffectReturnMode
    {
        ByParticleDuration,
        ByDelay
    }

    public sealed class PooledEffectPlayer : MonoBehaviour
    {
        [Header("Effect")]
        [SerializeField] private List<ParticleSystem> _prefabs;

        [Header("Pool")]
        [SerializeField] private int _poolCapacity = 10;

        [Header("Return")]
        [SerializeField] private EffectReturnMode _returnMode = EffectReturnMode.ByParticleDuration;
        [SerializeField] private float _returnDelay;

        private PoolBase<ParticleSystem> _pool;

        private void Awake()
        {
            if (_prefabs == null || _prefabs.Count == 0) return;
            _pool = new PoolBase<ParticleSystem>(CreateParticle, ActivateParticle, DeactivateParticle, _poolCapacity);
        }

        public void Play(Vector3 position)
        {
            PlayInternal(position).Forget();
        }

        private async UniTaskVoid PlayInternal(Vector3 position)
        {
            if (_pool == null)
                return;

            ParticleSystem particle = _pool.Get();

            if (particle == null)
                return;

            particle.transform.position = position;
            particle.Play();

            await UniTask.Delay((int)(GetLifetime(particle) * 1000));

            if (particle != null)
                _pool.Return(particle);
        }

        private float GetLifetime(ParticleSystem particle)
        {
            return _returnMode == EffectReturnMode.ByParticleDuration ? particle.main.duration : _returnDelay;
        }

        private ParticleSystem CreateParticle()
        {
            ParticleSystem particle = Instantiate(GetRandomPrefab());
            particle.gameObject.SetActive(false);
            return particle;
        }

        private static void ActivateParticle(ParticleSystem particle)
        {
            particle.gameObject.SetActive(true);
        }

        private static void DeactivateParticle(ParticleSystem particle)
        {
            particle.gameObject.SetActive(false);
        }

        private ParticleSystem GetRandomPrefab()
        {
            return _prefabs[Random.Range(0, _prefabs.Count)];
        }
    }
}