using Abstractions.Mechanics.Lifecycle;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Mechanics.UI
{
    public class UIStartLevelButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private UIStartPanel _startPanel;

        [Inject] private LevelStarter _levelStarter;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnStartButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnStartButtonClicked);
        }

        private void OnStartButtonClicked()
        {
            _levelStarter.ExecuteStart();
            _startPanel.Hide().Forget();
        }
    }
}