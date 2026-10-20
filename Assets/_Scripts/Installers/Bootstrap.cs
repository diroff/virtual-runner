using Abstractions.Interfaces;
using Reflex.Attributes;
using Reflex.Core;
using Services;
using UnityEngine;

namespace Installers
{
    public class Bootstrap : MonoBehaviour
    {
        private Container _container;

        [Inject]
        private async void Initialize(Container container)
        {
            _container = container;

            var services = _container.All<IService>();

            foreach (var service in services)
                await service.Initialize();

            var sceneManager = _container.Resolve<SceneManagerService>();
            await sceneManager.LoadMenuSceneAsync();
        }
    }
}