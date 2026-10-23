namespace Abstraction.Interfaces
{
    public interface IPoolable
    {
        void OnCreated();
        void OnSpawned();
        void OnDespawned();
    }
}