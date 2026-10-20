
using Abstractions.Interfaces;
using Cysharp.Threading.Tasks;

namespace Services
{
    public class LoadingScreenStubService : ILoadingScreenService
    {
        public UniTask Initialize()
        {
            return UniTask.CompletedTask;
        }

        public UniTask ShowAsync(float duration = 0.5F)
        {
            return UniTask.CompletedTask;
        }

        public UniTask HideAsync(float duration = 0.5F)
        {
            return UniTask.CompletedTask;
        }
    }
}
