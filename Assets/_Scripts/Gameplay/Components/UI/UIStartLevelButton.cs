using System.Threading;
using Abstractions.Mechanics.Lifecycle;
using Abstractions.UI;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Mechanics.UI
{
    public class UIStartLevelButton : UIButtonBase
    {
        [SerializeField] private UIStartPanel _startPanel;

        [Inject] private LevelStarter _levelStarter;

        protected override async UniTask OnClickAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _levelStarter.ExecuteStart();
            await _startPanel.Hide();
        }
    }
}
