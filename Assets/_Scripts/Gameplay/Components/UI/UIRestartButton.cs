using System.Threading;
using Services;
using Abstractions.UI;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Mechanics.UI
{
    public class UIRestartButton : UIButtonBase
    {
        [SerializeField] private UIEndPanel _exitPanel;

        [Inject] private SceneManagerService _sceneLoader;

        protected override async UniTask OnClickAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _exitPanel.Hide();
            _sceneLoader.ReloadScene();
        }
    }
}
