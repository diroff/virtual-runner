using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Mechanics.UI
{
    public class UIStartPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;

        public async UniTask Show()
        {
            _root.gameObject.SetActive(true);

            await UniTask.CompletedTask;
        }

        public async UniTask Hide()
        {
            _root.gameObject.SetActive(false);

            await UniTask.CompletedTask;
        }
    }
}