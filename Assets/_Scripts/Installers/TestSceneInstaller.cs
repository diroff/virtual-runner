using Reflex.Core;
using Services;
using UnityEngine;

namespace Installers
{
    public class TestSceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private PoolService _poolService;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(_poolService);
        }
    }
}