using Gameplay.Mechanics.Collectables;
using Gameplay.Mechanics.Creatures;
using Gameplay.Mechanics.LevelGeneration;
using Gameplay.Mechanics.Scoring;
using Reflex.Core;
using Services;
using UnityEngine;

namespace Installers
{
    public abstract class BaseGameplayInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private PoolService _poolService;
        [SerializeField] private LevelSegmentSpawner _levelSpawner;
        [SerializeField] private CoinCollectionHandler _coinCollectionHandler;
        [SerializeField] private DistanceScoreTracker _distanceScoreTracker;
        [SerializeField] private Player _player;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            RegisterBeforeLevelFlow(containerBuilder);
            RegisterLevelFlow(containerBuilder);
            RegisterAfterLevelFlow(containerBuilder);
        }

        protected virtual void RegisterBeforeLevelFlow(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(_poolService);
            containerBuilder.RegisterValue(_levelSpawner);
            containerBuilder.RegisterValue(_coinCollectionHandler);
            containerBuilder.RegisterValue(_distanceScoreTracker);
            containerBuilder.RegisterValue(_player);
        }

        protected abstract void RegisterLevelFlow(ContainerBuilder containerBuilder);

        protected virtual void RegisterAfterLevelFlow(ContainerBuilder containerBuilder)
        {

        }
    }
}