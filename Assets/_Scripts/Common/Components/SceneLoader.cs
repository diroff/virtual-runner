using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using Services;
using System.Threading;
using UnityEngine;

namespace Common.Components
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private string _sceneName;

        [Inject] private SceneManagerService _sceneManagerService;

        public void LoadScene()
        {
            LoadSceneAsync().Forget();
        }

        public void LoadScene(string sceneName)
        {
            LoadSceneAsync(sceneName).Forget();
        }

        public void LoadMenuScene()
        {
            LoadMenuSceneAsync().Forget();
        }

        public async UniTask LoadMenuSceneAsync(CancellationToken cancellationToken = default)
        {
            await _sceneManagerService.LoadMenuSceneAsync();
        }


        public async UniTask LoadSceneAsync(CancellationToken cancellationToken = default)
        {
            await LoadSceneAsync(_sceneName);
        }

        public async UniTask LoadSceneAsync(string sceneName, CancellationToken cancellationToken = default)
        {
            await _sceneManagerService.LoadSceneAsync(sceneName);
        }
    }
}