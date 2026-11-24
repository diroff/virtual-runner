using Abstractions.UI;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using Services;
using System.Threading;
using UnityEngine;

namespace Gameplay.Mechanics.UI
{
    public class UILoadSceneButton : UIButtonBase
    {
        [SerializeField] private string _sceneName;
        [Inject] private SceneManagerService _sceneLoader;

        protected override async UniTask OnClickAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _sceneLoader.LoadSceneAsync(_sceneName);
        }
    }
}