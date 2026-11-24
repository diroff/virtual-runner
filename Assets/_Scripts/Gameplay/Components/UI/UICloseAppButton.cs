using Abstractions.UI;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using Services;
using System.Threading;

namespace Gameplay.Mechanics.UI
{
    public class UICloseAppButton : UIButtonBase
    {
        [Inject] private SceneManagerService _sceneLoader;

        protected override async UniTask OnClickAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await UniTask.CompletedTask;
            _sceneLoader.QuitGame();
        }
    }
}