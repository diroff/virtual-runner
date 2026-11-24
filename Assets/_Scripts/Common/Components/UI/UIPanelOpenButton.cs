using System.Threading;
using Abstractions.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.Components.UI
{
    public sealed class UIPanelOpenButton : UIButtonBase
    {
        [SerializeField] private UIPanelBase _panel;

        protected override UniTask OnClickAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _panel != null ? _panel.ShowAsync() : UniTask.CompletedTask;
        }
    }
}
