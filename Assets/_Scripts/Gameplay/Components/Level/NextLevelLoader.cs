using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using Services;
using System.Threading;
using UnityEngine;

namespace Gameplay.Components.Level
{
    public class NextLevelLoader : MonoBehaviour
    {
        [SerializeField] private string _nextLevelName;
        [SerializeField] private bool _saveProgress = true;

        [Inject] private SaveLoadService _saveLoadService;
        [Inject] private SceneManagerService _sceneManagerService;

        public async UniTask LoadNextStageAsync(CancellationToken cancellationToken)
        {
            bool hasNextLevel = !string.IsNullOrEmpty(_nextLevelName);

            if (hasNextLevel)
            {
                if (_saveProgress)
                {
                    _saveLoadService.Data.Progress.LastReachedLevelName = _nextLevelName;
                    await _saveLoadService.SaveAsync();
                }

                await _sceneManagerService.LoadSceneAsync(_nextLevelName);
            }
            else
            {
                await _sceneManagerService.LoadMenuSceneAsync();
            }
        }
    }
}