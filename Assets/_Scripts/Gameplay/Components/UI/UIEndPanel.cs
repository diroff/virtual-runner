using Abstractions.Mechanics.Lifecycle;
using Cysharp.Threading.Tasks;
using Gameplay.Mechanics.Creatures;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Mechanics.UI
{
    public class UIEndPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Vector3 _canvasOffset;

        [Inject] private Player _player;
        [Inject] private LevelFinisher _levelFinisher;

        private void OnEnable()
        {
            _levelFinisher.OnBeforeFinished += OnLevelFinished;
        }

        private void OnDisable()
        {
            _levelFinisher.OnBeforeFinished -= OnLevelFinished;
        }

        public async UniTask Show()
        {
            _root.gameObject.SetActive(true);
            SetCanvasToPlayerPosition();

            await UniTask.CompletedTask;
        }

        public async UniTask Hide()
        {
            _root.gameObject.SetActive(false);

            await UniTask.CompletedTask;
        }

        public void SetCanvasToPlayerPosition()
        {
            var position = _player.transform.position + _canvasOffset;

            _canvas.transform.position = position;
        }

        private void OnLevelFinished()
        {
            Show().Forget();
        }
    }
}