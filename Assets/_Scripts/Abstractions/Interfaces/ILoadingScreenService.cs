using Cysharp.Threading.Tasks;

namespace Abstractions.Interfaces
{
    public interface ILoadingScreenService : IService
    {
        public UniTask ShowAsync(float duration = 0.5f);
        public UniTask HideAsync(float duration = 0.5f);
    }
}