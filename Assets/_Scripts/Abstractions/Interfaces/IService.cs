using Cysharp.Threading.Tasks;

namespace Abstractions.Interfaces
{
    public interface IService
    {
        public UniTask Initialize();
    }
}