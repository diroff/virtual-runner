using Abstractions.UI;
using UnityEngine;

namespace Common.Components.UI
{
    public sealed class UIPanel : UIPanelBase
    {
        [SerializeField] private UIPanelNavigationType _navigationType;

        public override UIPanelNavigationType NavigationType => _navigationType;
    }
}
