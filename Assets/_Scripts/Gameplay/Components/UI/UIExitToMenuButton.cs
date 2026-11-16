using Cysharp.Threading.Tasks;
using Gameplay.Components.Level;
using Reflex.Attributes;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Mechanics.UI
{
    public class UIExitToMenuButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private UIStartPanel _startPanel;

        [Inject] private SceneManagerService _sceneLoader;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnExitButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnExitButtonClicked);
        }

        private void OnExitButtonClicked()
        {
            _sceneLoader.LoadMenuScene();
            _startPanel.Hide().Forget();
        }
    }
}