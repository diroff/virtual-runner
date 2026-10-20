using Reflex.Core;
using UnityEngine;

namespace Installers
{
    public class ProjectInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private string _menuSceneName;

        public void InstallBindings(ContainerBuilder builder)
        {

        }
    }
}