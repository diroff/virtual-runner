using System.Threading;
using Abstractions.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.Components.UI
{
    public sealed class UIPanelBackButton : UIButtonBase
    {
        [SerializeField] private UIPanelNavigator _navigator;

        protected override UniTask OnClickAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _navigator != null ? _navigator.BackAsync() : UniTask.CompletedTask;
        }
    }
}
