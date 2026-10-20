namespace Abstractions.Interfaces
{
    public interface IInventoryItem
    {
        public string Id { get; }
        public int MaxStack { get; }
        public bool IsStackable { get; }
    }
}