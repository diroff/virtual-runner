using Cysharp.Threading.Tasks;

namespace Abstractions.Interfaces
{
    public interface IStorageService
    {
        public UniTask SaveAsync<T>(string key, T data);
        public UniTask<T> LoadAsync<T>(string key);
    }
}