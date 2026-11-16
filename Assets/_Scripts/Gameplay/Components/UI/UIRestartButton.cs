using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Mechanics.UI
{
    public class UIRestartButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private UIEndPanel _exitPanel;

        [Inject] private SceneManagerService _sceneLoader;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnRestartButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnRestartButtonClicked);
        }

        private void OnRestartButtonClicked()
        {
            _sceneLoader.ReloadScene();
            _exitPanel.Hide().Forget();
        }
    }
}