using System;
using Abstractions.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services
{
    public class SceneManagerService : IService
    {
        private readonly string _menuSceneName;
        private readonly LoadingScreenService _loadingScreenService;
        private bool _isLoading;

        public SceneManagerService(string menuSceneName, LoadingScreenService loadingScreenService)
        {
            _menuSceneName = menuSceneName;
            _loadingScreenService = loadingScreenService;
        }

        public UniTask Initialize()
        {
            return UniTask.CompletedTask;
        }

        public void LoadScene(string name)
        {
            SceneManager.LoadScene(name);
        }

        public void LoadScene(int index)
        {
            SceneManager.LoadScene(index);
        }

        public async UniTask LoadSceneAsync(string name, float fadeDuration = 1f)
        {
            await LoadSceneInternalAsync(() => SceneManager.LoadSceneAsync(name), fadeDuration);
        }

        public async UniTask LoadSceneAsync(int index, float fadeDuration = 1f)
        {
            await LoadSceneInternalAsync(() => SceneManager.LoadSceneAsync(index), fadeDuration);
        }

        private async UniTask LoadSceneInternalAsync(Func<AsyncOperation> loadAction, float fadeDuration)
        {
            if (_isLoading) 
                return;

            _isLoading = true;

            try
            {
                await _loadingScreenService.FadeInAsync(fadeDuration);

                var loadOperation = loadAction();
                loadOperation.allowSceneActivation = true;

                while (!loadOperation.isDone)
                {
                    await UniTask.Yield();
                }

                await UniTask.Yield(PlayerLoopTiming.Update);

                await _loadingScreenService.FadeOutAsync(fadeDuration);
            }
            finally
            {
                _isLoading = false;
            }
        }

        public void LoadMenuScene()
        {
            LoadScene(_menuSceneName);
        }

        public async UniTask LoadMenuSceneAsync(float fadeDuration = 1f)
        {
            await LoadSceneAsync(_menuSceneName, fadeDuration);
        }

        public void ReloadScene()
        {
            LoadScene(GetActiveScene());
        }

        public async UniTask ReloadSceneAsync(float fadeDuration = 1f)
        {
            await LoadSceneAsync(GetActiveScene(), fadeDuration);
        }

        private int GetActiveScene()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}