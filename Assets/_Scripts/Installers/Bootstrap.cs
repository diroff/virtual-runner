using Reflex.Attributes;
using System.ComponentModel;
using UnityEngine;

namespace Installers
{
    public class Bootstrap : MonoBehaviour
    {
        [Inject] private Container _container;

        private async void Awake()
        {
            
        }
    }
}