using Abstractions.Components.Movement;
using Abstractions.Interfaces;
using Abstractions.Mechanics.Lifecycle;
using Gameplay.Mechanics.Lifecycle;
using Reflex.Core;
using UnityEngine;

namespace Installers
{
    public class GameplayClassicInstaller : BaseGameplayInstaller
    {
        [SerializeField] private GameplayClassicLevelStarter _levelStarter;
        [SerializeField] private GameplayClassicLevelFinisher _levelFinisher;

        protected override void RegisterBeforeLevelFlow(ContainerBuilder containerBuilder)
        {
            base.RegisterBeforeLevelFlow(containerBuilder);

            containerBuilder.RegisterValue(_levelStarter, new[]
{
                typeof(LevelStarter),
                _levelStarter.GetType()
            });

            containerBuilder.RegisterValue(_levelFinisher, new[]
{
                typeof(LevelFinisher),
                _levelFinisher.GetType()
            });
        }

        protected override void RegisterLevelFlow(ContainerBuilder containerBuilder)
        {
            
        }
    }
}