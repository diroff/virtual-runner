using Abstractions.Components.Movement;
using Gameplay.Components.Movement;
using Gameplay.Mechanics.Collectables;
using Gameplay.Mechanics.Creatures;
using Gameplay.Mechanics.LevelGeneration;
using Gameplay.Mechanics.PowerUps;
using Gameplay.Mechanics.Progression;
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

        [SerializeField] private MovementDistanceTracker _movementDistanceTracker;
        [SerializeField] private MovementBase _movement;
        [SerializeField] private MovementSpeed _movementSpeed;
        [SerializeField] private PowerUpHandler _powerUpHandler;
        [SerializeField] private PowerUpContext _powerUpContext;

        [SerializeField] private SegmentDifficultyDirector _segmentDifficultyDirector;
        [SerializeField] private LevelPowerUpSpawner _levelPowerUpSpawner;
        [SerializeField] private MovementSpeedDirector _movementSpeedDirector;
        [SerializeField] private ScoreMultiplierDirector _scoreMultiplierDirector;

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

            containerBuilder.RegisterValue(_movementDistanceTracker);
            containerBuilder.RegisterValue(_movement, new[]
            {
                typeof(MovementBase),
                _movement.GetType()
            });

            containerBuilder.RegisterValue(_movementSpeed);
            containerBuilder.RegisterValue(_powerUpHandler);
            containerBuilder.RegisterValue(_powerUpContext);

            containerBuilder.RegisterValue(_segmentDifficultyDirector);
            containerBuilder.RegisterValue(_levelPowerUpSpawner);
            containerBuilder.RegisterValue(_movementSpeedDirector);
            containerBuilder.RegisterValue(_scoreMultiplierDirector);
        }

        protected abstract void RegisterLevelFlow(ContainerBuilder containerBuilder);

        protected virtual void RegisterAfterLevelFlow(ContainerBuilder containerBuilder)
        {
        }
    }
}
