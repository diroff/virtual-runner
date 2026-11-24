using System.Threading;
using Services;
using Abstractions.UI;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Mechanics.UI
{
    public class UIExitToMenuButton : UIButtonBase
    {
        [SerializeField] private UIPanelBase _panel;

        [Inject] private SceneManagerService _sceneLoader;

        protected override async UniTask OnClickAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _panel.Hide();
            _sceneLoader.LoadMenuScene();
        }
    }
}
