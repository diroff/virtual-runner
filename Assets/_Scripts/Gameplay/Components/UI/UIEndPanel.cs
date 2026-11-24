using Abstractions.Mechanics.Lifecycle;
using Abstractions.UI;
using Cysharp.Threading.Tasks;
using Gameplay.Mechanics.Creatures;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Mechanics.UI
{
    public class UIEndPanel : UIPanelBase
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Vector3 _canvasOffset;

        [Inject] private Player _player;
        [Inject] private LevelFinisher _levelFinisher;

        public override UIPanelNavigationType NavigationType => UIPanelNavigationType.Popup;

        private LevelFinisher _subscribedFinisher;

        protected override void OnEnable()
        {
            base.OnEnable();
            Subscribe();
        }

        private void Start()
        {
            Subscribe();
        }

        protected override void OnDisable()
        {
            Unsubscribe();
            base.OnDisable();
        }

        protected override void OnBeforeShow()
        {
            SetCanvasToPlayerPosition();
        }

        public void SetCanvasToPlayerPosition()
        {
            if (_player == null || _canvas == null)
                return;

            var position = _player.transform.position + _canvasOffset;
            _canvas.transform.position = position;
        }

        private void Subscribe()
        {
            if (_subscribedFinisher == _levelFinisher || _levelFinisher == null)
                return;

            Unsubscribe();
            _subscribedFinisher = _levelFinisher;
            _subscribedFinisher.OnBeforeFinished += OnLevelFinished;
        }

        private void Unsubscribe()
        {
            if (_subscribedFinisher != null)
                _subscribedFinisher.OnBeforeFinished -= OnLevelFinished;

            _subscribedFinisher = null;
        }

        private void OnLevelFinished()
        {
            if (IsOpen && !IsTransitioning)
                SetCanvasToPlayerPosition();

            Root.gameObject.SetActive(true);
            Show().Forget();
        }
    }
}
