using Abstractions.Interfaces;
using Reflex.Core;
using Reflex.Enums;
using Services;
using UnityEngine;
using Resolution = Reflex.Enums.Resolution;

namespace Installers
{
    public class ProjectInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private string _menuSceneName;

        public void InstallBindings(ContainerBuilder builder)
        {
            builder.RegisterType(typeof(LoadingScreenStubService), 
                new[] { typeof(ILoadingScreenService), typeof(IService) }, 
                Lifetime.Singleton, Resolution.Lazy);

            builder.RegisterFactory(container => new SceneManagerService(_menuSceneName,container.Resolve<ILoadingScreenService>()),
                new[] { typeof(SceneManagerService), typeof(IService) },
                Lifetime.Singleton,
                Resolution.Lazy);
        }
    }
}
